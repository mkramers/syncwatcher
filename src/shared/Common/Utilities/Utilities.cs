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
        private static readonly ILog LOG = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                    for (int i = 0; i < palette.Entries.Length; i++)
                    {
                        palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                    }
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

        public static Dictionary<string, T> GetFieldValues<T>(Type _type)
        {
            return _type.GetFields().Where(p => p.FieldType == typeof(T)).ToDictionary(f => f.Name, f => (T) f.GetValue(null));
        }

        public static Dictionary<string, T> GetPropertyValues<T>(Type _type)
        {
            PropertyInfo[] properties = _type.GetProperties();

            return properties.Where(p => p.PropertyType == typeof(T)).ToDictionary(f => f.Name, f => (T) f.GetValue(null));
        }

        public static T[] GetEnumValues<T>()
        {
            return (T[]) Enum.GetValues(typeof(T));
        }

        public static void ConvertTo8BitGrayscalePalette(Image _image)
        {
            if (_image != null)
            {
                _image.Palette = Format8BppIndexedToGrayscale_Palette;
            }
        }

        public static SecureString GetSecureString(string strPassword)
        {
            SecureString secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (char c in strPassword)
                {
                    secureStr.AppendChar(c);
                }
            }
            return secureStr;
        }

        public static Color SetTransparency(byte _alpha, Color color)
        {
            return Color.FromArgb(_alpha, color.R, color.G, color.B);
        }

        public static float[] ToFloatArray(this Vector4 _vector4)
        {
            float[] array = new float[4];
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
            {
                return;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(_serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
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
            {
                return default(T);
            }

            T objectOut = default(T);

            StringReader read = null;

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(_fileName);
                string xmlString = xmlDocument.OuterXml;

                read = new StringReader(xmlString);
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);

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
            {
                return;
            }

            try
            {
                FileStream stream = File.Create(_fileName);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, _serializableObject);
                stream.Close();
            }
            catch (Exception)
            {
            }
        }

        public static T BinaryDeserializeObject<T>(string _fileName)
        {
            T deserializedObject = default(T);

            try
            {
                FileStream stream = File.OpenRead(_fileName);
                BinaryFormatter formatter = new BinaryFormatter();
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
            string extension = "invalidextension";

            string[] splitName = _fileName.Split('.');

            if (splitName != null && splitName.Length > 0)
            {
                extension = splitName[splitName.Length - 1];
            }

            return extension;
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static List<List<T>> GetValuesFromFile<T>(string _fileName, params char[] _seperator)
        {
            List<List<T>> list = new List<List<T>>();

            if (!File.Exists(_fileName))
            {
                LOG.ErrorFormat("GetValuesFromFile failed: File {0} does not exist. Returning...", _fileName);
                return list;
            }

            List<string[]> lines = GetFileLinesSplit(_fileName, _seperator);

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            foreach (string[] line in lines)
            {
                List<T> row = new List<T>();
                foreach (string item in line)
                {
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        T value = (T) converter.ConvertFrom(item);
                        row.Add(value);
                    }
                }

                list.Add(row);
            }

            return list;
        }

        public static List<string[]> GetFileLinesSplit(string _fileName, params char[] _seperator)
        {
            List<string[]> contents = new List<string[]>();

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

            List<string> lines = new List<string>();
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
                foreach (string line in lines)
                {
                    contents.Add(line.Split(_seperator, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            return contents;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException($"Expresion '{propertyLambda}' refers to a property that is not from type {type}.");
            }

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
            {
                throw new NotImplementedException();
            }

            FileAttributes attributes = File.GetAttributes(_info.FullName);

            bool containsAttribute = attributes.HasFlag(_attribute);
            return containsAttribute;
        }
    }
}
