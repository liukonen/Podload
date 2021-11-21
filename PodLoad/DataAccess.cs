using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace podload
{
    class DataAccess
    {
        public Settings LoadXML(string FileName)
        {
            if (File.Exists(FileName))
            {
                return Load(FileName, false);
            }
            else
            {
                Console.WriteLine("Unable to load file, aborting");
                Console.Beep();
                Console.ReadLine();
            }
            return null;
        }
        public Settings LoadObject(string filename)
        {
            return Load(filename, true);
        }
        public void SaveXml(Settings S, string FileName) { Save(S, FileName, false); }
        public void SaveObject(Settings s, string filename) { Save(s, filename, true); }

        private void Save(Settings setting, string fileName, bool compresionEnabled)
        {
            XmlSerializer Serializer = new XmlSerializer(setting.GetType());
            StringBuilder Builder = new StringBuilder();
            _ = System.Xml.XmlWriter.Create(Builder, GetWriterSettings());

            using (FileStream F = new FileStream(fileName, FileMode.Create))
            {
                using (DeflateStream gz = new DeflateStream(F, CompressionMode.Compress, false))
                {
                    if (compresionEnabled) { Serializer.Serialize(gz, setting); }
                    else { Serializer.Serialize(F, setting); }
                }

            }
        }
        private Settings Load(string fileName, bool compressionEnabled)
        {
            Settings value = new Settings();
            XmlSerializer Serializer = new XmlSerializer(value.GetType());

            using (var outputFile = new FileStream(fileName, FileMode.Open))
            {
                if (compressionEnabled)
                {
                    using (var compressionStream = new DeflateStream(outputFile, System.IO.Compression.CompressionMode.Decompress))
                    {
                        value = (Settings)Serializer.Deserialize(compressionStream);
                    }
                }
                else { value = (Settings)Serializer.Deserialize(outputFile); }
            }
            return value;
        }
        private System.Xml.XmlWriterSettings GetWriterSettings()
        {
            System.Xml.XmlWriterSettings xSettings = new System.Xml.XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            };
            return xSettings;
        }
    }
}
