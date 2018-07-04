using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace P4Commands
{
    public static class Utilities
    {
        ///
        ///Serializes an object.
        /// 
        ///@param T the type of object to serialize
        ///@param _serializableObject the object to serialize
        ///@param _filename the path at which to store the serialized object
        public static void XmlSerializeObject<T>(T _serializableObject, string _fileName)
        {
            if (_serializableObject == null || String.IsNullOrEmpty(_fileName))
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

        ///
        ///De-Serializes an object and returns the result
        /// 
        ///@param T the type of object to deserialize
        ///@param _filename the path at which to read the serialized object from
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
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(_fileName);
                string xmlString = xmlDocument.OuterXml;

                read = new StringReader(xmlString);
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);

                XmlReader reader = new XmlTextReader(read);
                objectOut = (T)serializer.Deserialize(reader);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {

            }
            finally
            {
                read?.Dispose();
            }
            return objectOut;
        }
    }
}
