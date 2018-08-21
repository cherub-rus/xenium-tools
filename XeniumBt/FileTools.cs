using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace XeniumBt {

    internal static class FileTools {

        public static void Serialize(string filename, object data) {
            FileStream fileStream =
                new FileStream(filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
            XmlSerializer s = new XmlSerializer(data.GetType());
            s.Serialize(sw, data);
            sw.Close();
            fileStream.Close();
        }

        public static object Deserialize(string filename, Type type) {
            FileStream fileStream =
                new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fileStream, Encoding.UTF8);
            try {
                XmlSerializer s = new XmlSerializer(type);
                return s.Deserialize(sr);
            } finally {
                sr.Close();
                fileStream.Close();
            }
        }

        public static void WriteLog(string fileName, string message) {
            File.AppendAllLines(fileName, new []{ message }, Encoding.UTF8);
        }

    }
}