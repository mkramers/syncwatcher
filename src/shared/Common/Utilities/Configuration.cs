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
        private static readonly Dictionary<string, List<KeyValuePair<string, string>>> m_map = new Dictionary<string, List<KeyValuePair<string, string>>>();

        private static object m_sync;

        private static readonly ILog LOG = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool SilentMode { get; set; }

        static ConfigurationManager2()
        {
            SilentMode = true;
        }

        public static bool Initialize(string _filePath)
        {
            bool success = false;

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
                    string message = string.Format(CultureInfo.CurrentCulture, "Invalid configuration file: {0}.\nException = {1}", _filePath, e.Message);
                    LOG.Warn(message);
                }
            }

            return success;
        }

        private static void LoadConfiguration(string _filePath)
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, "Configuration file not found at: {0}", Path.GetFullPath(_filePath)));
            }

            LOG.DebugFormat("Loading configuration from file: {0}", Path.GetFullPath(_filePath));

            using (StreamReader file = new StreamReader(_filePath))
            {
                string line = string.Empty;
                while ((line = file.ReadLine()) != null)
                {
                    //skip comment or empty line
                    if (line.StartsWith("#", StringComparison.CurrentCulture) || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] split = line.Split('=');
                    if (split.Length == 2)
                    {
                        string key = split[0];
                        string value = split[1];
                        string member = key.Split('.').Last();

                        int index = key.LastIndexOf('.');
                        key = key.Substring(0, index);

                        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(member, value);

                        if (m_map.ContainsKey(key))
                        {
                            if (m_map[key] == null)
                            {
                                m_map[key] = new List<KeyValuePair<string, string>>();
                            }
                        }
                        else
                        {
                            m_map.Add(key, new List<KeyValuePair<string, string>>());
                        }

                        m_map[key].Add(keyValuePair);

                        if (!SilentMode)
                        {
                            LOG.DebugFormat("Found configuration key: {0} : {1} : {2}", key, member, value);
                        }
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
            {
                throw new ArgumentNullException(nameof(_class), "Class is null in ConfigurationManager.Configure");
            }

            bool found = false;

            Type type = null;

            //if the class is a type (i.e. a static class can be configured by passing in its type)
            if (_class is Type)
            {
                type = _class as Type;
            }
            else
            {
                type = _class.GetType();
            }

            string typeName = type.FullName;

            if (!SilentMode)
            {
                LOG.DebugFormat("Configuring class: {0}", typeName);
            }

            //if our map contains this class then find the property and set it
            if (m_map.ContainsKey(typeName) && m_map[typeName] != null)
            {
                PropertyInfo[] properties = type.GetProperties();

                List<KeyValuePair<string, string>> memberValuePair = m_map[typeName];

                foreach (PropertyInfo property in properties)
                {
                    KeyValuePair<string, string> result = memberValuePair.Find(i => i.Key == property.Name);
                    if (!result.Equals(default(KeyValuePair<string, string>)))
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);

                        object value = null;

                        //auto convert string to string array
                        if (property.PropertyType.IsArray)
                        {
                            Type elementType = property.PropertyType.GetElementType();
                            TypeConverter elementConverter = TypeDescriptor.GetConverter(elementType);

                            string[] split = result.Value.Split(',');
                            Array results = Array.CreateInstance(elementType, split.Length);
                            for (int i = 0; i < split.Length; i++)
                            {
                                results.SetValue(elementConverter.ConvertFromString(split[i]), i);
                            }

                            value = results;
                        }
                        else
                        {
                            value = typeConverter.ConvertFromString(result.Value);
                        }

                        property.SetValue(_class, value);
                        found = true;

                        if (!SilentMode)
                        {
                            LOG.DebugFormat("Configuring {0} with value {1} in class {2}", property.Name, result.Value, typeName);
                        }
                    }
                }
            }

            if (!found && !SilentMode)
            {
                LOG.DebugFormat("No properties for {0} found in configuration file", typeName);
            }
        }

        public static void Dump(string _filePath)
        {
            LOG.DebugFormat("Creating configuration dump file at: {0}", _filePath);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<string, List<KeyValuePair<string, string>>> classMemberPair in m_map)
            foreach (KeyValuePair<string, string> memberValuePair in classMemberPair.Value)
            {
                string text = string.Format(CultureInfo.CurrentCulture, "{0}.{1}={2}\n", classMemberPair.Key, memberValuePair.Key, memberValuePair.Value);
                LOG.DebugFormat("Adding key pair to dump file: {0}", text);
                stringBuilder.Append(text);
            }

            File.WriteAllText(_filePath, stringBuilder.ToString());
        }
    }
}
