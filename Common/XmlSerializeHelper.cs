using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common
{
    public static class XmlSerializeHelper
    {
        #region Serialization
        public static T FromXml<T>(string filename) where T : class
        {
            using (var fileStream = File.OpenRead(filename))
            {
                return FromXml<T>(fileStream);
            }
        }

        public static T FromXml<T>(Stream stream) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            return ser.Deserialize(stream) as T;
        }

        public static void ToXml<T>(string filename, T data) where T : class
        {
            using (var fileStream = File.Open(filename, FileMode.Create))
            {
                ToXml(fileStream, data);
            }
        }

        public static void ToXml<T>(Stream stream, T data)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            ser.Serialize(stream, data);
        }
        #endregion
    }
}
