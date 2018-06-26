using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace Common
{
    public static class ConfigurationManager2
    {
        static ConfigurationManager2()
        {
            SilentMode = true;
        }

        public static bool Initialize(string _filePath)
        {
            var success = false;

            m_sync = new object();

            lock (m_sync)
            {
                try
                {
                    LoadConfiguration(_filePath);
                    success = true;
                }
                catch (Exception e)
                {
                    var message = string.Format(CultureInfo.CurrentCulture,
                        "Invalid configuration file: {0}.\nException = {1}", _filePath, e.Message);
                    LOG.Warn(message);
                }
            }

            return success;
        }

        private static void LoadConfiguration(string _filePath)
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture,
                    "Configuration file not found at: {0}", Path.GetFullPath(_filePath)));

            LOG.DebugFormat("Loading configuration from file: {0}", Path.GetFullPath(_filePath));

            using (var file = new StreamReader(_filePath))
            {
                var line = string.Empty;
                while ((line = file.ReadLine()) != null)
                {
                    //skip comment or empty line
                    if (line.StartsWith("#", StringComparison.CurrentCulture) || string.IsNullOrWhiteSpace(line))
                        continue;

                    var split = line.Split('=');
                    if (split.Length == 2)
                    {
                        var key = split[0];
                        var value = split[1];
                        var member = key.Split('.').Last();

                        var index = key.LastIndexOf('.');
                        key = key.Substring(0, index);

                        var keyValuePair = new KeyValuePair<string, string>(member, value);

                        if (m_map.ContainsKey(key))
                        {
                            if (m_map[key] == null)
                                m_map[key] = new List<KeyValuePair<string, string>>();
                        }
                        else
                        {
                            m_map.Add(key, new List<KeyValuePair<string, string>>());
                        }

                        m_map[key].Add(keyValuePair);

                        if (!SilentMode)
                            LOG.DebugFormat("Found configuration key: {0} : {1} : {2}", key, member, value);
                    }
                    else
                    {
                        throw new InvalidOperationException("Load Configuration parsed an invalid line");
                    }
                }
            }
        }

        public static void Configure(object _class)
        {
            if (_class == null)
                throw new ArgumentNullException(nameof(_class), "Class is null in ConfigurationManager.Configure");

            var found = false;

            Type type = null;

            //if the class is a type (i.e. a static class can be configured by passing in its type)
            if (_class is Type)
                type = _class as Type;
            else
                type = _class.GetType();

            var typeName = type.FullName;

            if (!SilentMode)
                LOG.DebugFormat("Configuring class: {0}", typeName);

            //if our map contains this class then find the property and set it
            if (m_map.ContainsKey(typeName) && m_map[typeName] != null)
            {
                var properties = type.GetProperties();

                var memberValuePair = m_map[typeName];

                foreach (var property in properties)
                {
                    var result = memberValuePair.Find(i => i.Key == property.Name);
                    if (!result.Equals(default(KeyValuePair<string, string>)))
                    {
                        var typeConverter = TypeDescriptor.GetConverter(property.PropertyType);

                        object value = null;

                        //auto convert string to string array
                        if (property.PropertyType.IsArray)
                        {
                            var elementType = property.PropertyType.GetElementType();
                            var elementConverter = TypeDescriptor.GetConverter(elementType);

                            var split = result.Value.Split(',');
                            var results = Array.CreateInstance(elementType, split.Length);
                            for (var i = 0; i < split.Length; i++)
                                results.SetValue(elementConverter.ConvertFromString(split[i]), i);

                            value = results;
                        }
                        else
                        {
                            value = typeConverter.ConvertFromString(result.Value);
                        }

                        property.SetValue(_class, value);
                        found = true;

                        if (!SilentMode)
                            LOG.DebugFormat("Configuring {0} with value {1} in class {2}", property.Name, result.Value,
                                typeName);
                    }
                }
            }

            if (!found && !SilentMode)
                LOG.DebugFormat("No properties for {0} found in configuration file", typeName);
        }

        public static void Dump(string _filePath)
        {
            LOG.DebugFormat("Creating configuration dump file at: {0}", _filePath);

            var stringBuilder = new StringBuilder();

            foreach (var classMemberPair in m_map)
            foreach (var memberValuePair in classMemberPair.Value)
            {
                var text = string.Format(CultureInfo.CurrentCulture, "{0}.{1}={2}\n", classMemberPair.Key,
                    memberValuePair.Key, memberValuePair.Value);
                LOG.DebugFormat("Adding key pair to dump file: {0}", text);
                stringBuilder.Append(text);
            }

            File.WriteAllText(_filePath, stringBuilder.ToString());
        }

        public static bool SilentMode { get; set; }

        private static readonly Dictionary<string, List<KeyValuePair<string, string>>> m_map =
            new Dictionary<string, List<KeyValuePair<string, string>>>();

        private static object m_sync;

        private static readonly ILog LOG = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}