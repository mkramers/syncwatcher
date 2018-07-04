using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using Color = System.Windows.Media.Color;

namespace Common
{
    public static class Utilities
    {
        public static Dictionary<string, T> GetFieldValues<T>(Type _type)
        {
            return _type
                .GetFields()
                .Where(p => p.FieldType == typeof(T))
                .ToDictionary(f => f.Name,
                    f => (T) f.GetValue(null));
        }

        public static Dictionary<string, T> GetPropertyValues<T>(Type _type)
        {
            var properties = _type
                .GetProperties();

            return properties
                .Where(p => p.PropertyType == typeof(T))
                .ToDictionary(f => f.Name,
                    f => (T) f.GetValue(null));
        }

        public static T[] GetEnumValues<T>()
        {
            return (T[]) Enum.GetValues(typeof(T));
        }

        public static void ConvertTo8BitGrayscalePalette(Image _image)
        {
            if (_image != null)
                _image.Palette = Format8BppIndexedToGrayscale_Palette;
        }

        public static SecureString GetSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            return secureStr;
        }

        public static Color SetTransparency(byte _alpha, Color color)
        {
            return Color.FromArgb(_alpha, color.R, color.G, color.B);
        }

        public static float[] ToFloatArray(this Vector4 _vector4)
        {
            var array = new float[4];
            array[0] = _vector4.X;
            array[1] = _vector4.Y;
            array[2] = _vector4.Z;
            array[3] = _vector4.W;
            return array;
        }

        /// Serializes an object.
        ///  
        /// @param T the type of object to serialize
        /// @param _serializableObject the object to serialize
        /// @param _filename the path at which to store the serialized object
        public static void XmlSerializeObject<T>(T _serializableObject, string _fileName)
        {
            if (_serializableObject == null || string.IsNullOrEmpty(_fileName))
                return;

            try
            {
                var xmlDocument = new XmlDocument();
                var serializer = new XmlSerializer(_serializableObject.GetType());
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, _serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(_fileName);
                    //stream.Close();
                }
            }
            catch (FileNotFoundException)
            {
            }
            catch (ArgumentException)
            {
            }
        }

        /// De-Serializes an object and returns the result
        ///  
        /// @param T the type of object to deserialize
        /// @param _filename the path at which to read the serialized object from
        public static T XmlDeserializeObject<T>(string _fileName)
        {
            // early out!
            if (string.IsNullOrWhiteSpace(_fileName))
                return default(T);

            var objectOut = default(T);

            StringReader read = null;

            try
            {
                var attributeXml = string.Empty;

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(_fileName);
                var xmlString = xmlDocument.OuterXml;

                read = new StringReader(xmlString);
                var outType = typeof(T);

                var serializer = new XmlSerializer(outType);

                XmlReader reader = new XmlTextReader(read);
                objectOut = (T) serializer.Deserialize(reader);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to deserialize!\n{e}");
            }
            finally
            {
                read?.Dispose();
            }
            return objectOut;
        }

        public static void BinarySerializeObject<T>(T _serializableObject, string _fileName)
        {
            if (_serializableObject == null || string.IsNullOrEmpty(_fileName))
                return;

            try
            {
                var stream = File.Create(_fileName);
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, _serializableObject);
                stream.Close();
            }
            catch (Exception)
            {
            }
        }

        public static T BinaryDeserializeObject<T>(string _fileName)
        {
            var deserializedObject = default(T);

            try
            {
                var stream = File.OpenRead(_fileName);
                var formatter = new BinaryFormatter();
                deserializedObject = (T) formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception)
            {
            }

            return deserializedObject;
        }

        public static string GetExtensionFromFileName(string _fileName)
        {
            var extension = "invalidextension";

            var splitName = _fileName.Split('.');

            if (splitName != null && splitName.Length > 0)
                extension = splitName[splitName.Length - 1];

            return extension;
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static List<List<T>> GetValuesFromFile<T>(string _fileName, params char[] _seperator)
        {
            var list = new List<List<T>>();

            if (!File.Exists(_fileName))
            {
                LOG.ErrorFormat("GetValuesFromFile failed: File {0} does not exist. Returning...", _fileName);
                return list;
            }

            var lines = GetFileLinesSplit(_fileName, _seperator);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            foreach (var line in lines)
            {
                var row = new List<T>();
                foreach (var item in line)
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        var value = (T) converter.ConvertFrom(item);
                        row.Add(value);
                    }

                list.Add(row);
            }

            return list;
        }

        public static List<string[]> GetFileLinesSplit(string _fileName, params char[] _seperator)
        {
            var contents = new List<string[]>();

            //if (!File.Exists(_fileName))
            //{
            //    LOG.ErrorFormat("GetFileLinesSplit failed: File {0} does not exist. Returning...", _fileName);
            //    return contents;
            //}
            if (_seperator == null)
            {
                LOG.ErrorFormat("GetFileLinesSplit failed: Seperator is null. Returning...", _fileName);
                return contents;
            }

            var lines = new List<string>();
            try
            {
                lines.AddRange(File.ReadAllLines(_fileName).ToList());
            }
            catch (Exception e)
            {
                LOG.ErrorFormat("GetFileLinesSplit failed reading from {0}: {1}", _fileName, e.Message);
            }
            finally
            {
                foreach (var line in lines)
                    contents.Add(line.Split(_seperator, StringSplitOptions.RemoveEmptyEntries));
            }

            return contents;
        }

        public static StaticAnalysisWarning GetStaticAnalysisWarning(List<string> _data)
        {
            StaticAnalysisWarning warning = null;

            if (_data != null)
            {
                var line = 0;
                int.TryParse(_data[4], out line);

                warning = new StaticAnalysisWarning
                {
                    Warning = _data[0],
                    Desciprtion = _data[1],
                    Project = _data[2],
                    File = _data[3],
                    Line = line
                };
            }

            return warning;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(
                    $"Expresion '{propertyLambda}' refers to a property that is not from type {type}.");

            return propInfo;
        }

        public static bool IsFile(FileSystemInfo _info)
        {
            return !ContainsAttribute(_info, FileAttributes.Directory);
        }

        public static bool IsDirectory(FileSystemInfo _info)
        {
            return ContainsAttribute(_info, FileAttributes.Directory);
        }

        public static bool ContainsAttribute(FileSystemInfo _info, FileAttributes _attribute)
        {
            Debug.Assert(_info != null);
            if (!_info.Exists)
                throw new NotImplementedException();

            var attributes = File.GetAttributes(_info.FullName);

            var containsAttribute = attributes.HasFlag(_attribute);
            return containsAttribute;
        }

        public static ColorPalette Format8BppIndexedToGrayscale_Palette
        {
            get
            {
                ColorPalette palette = null;
                Bitmap bmp = null;

                try
                {
                    //create a dummy image so we can get a palette from it
                    bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);

                    //create the new palette
                    palette = bmp.Palette;

                    //convert the palette to grayscale
                    for (var i = 0; i < palette.Entries.Length; i++)
                        palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                }
                catch (ArgumentException)
                {
                }
                finally
                {
                    bmp?.Dispose();
                }

                return palette;
            }
        }

        public static string Divider => "-------------------------------------------------";

        private static readonly ILog LOG = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}