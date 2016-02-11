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
            Settings S = new Settings();
            if (File.Exists(FileName))
            {
                XmlSerializer Serializer = new XmlSerializer(S.GetType());
                S = (Settings)Serializer.Deserialize(File.OpenRead(FileName));
            }
            else
            {
                Console.WriteLine("Unable to load file, aborting");
                Console.Beep();
                Console.ReadLine();
            }
            return S;
        }

        public void saveXml(Settings S, string FileName)
        {
            XmlSerializer Serializer = new XmlSerializer(S.GetType());
            StringBuilder Builder = new StringBuilder();
            System.Xml.XmlWriterSettings xSettings = new System.Xml.XmlWriterSettings();
            xSettings.Encoding = Encoding.UTF8;
            xSettings.OmitXmlDeclaration = true;
            System.Xml.XmlWriter Writer = System.Xml.XmlWriter.Create(Builder, xSettings);
            Serializer.Serialize(Writer, S);
            Writer.Close();
            System.IO.StreamWriter Writer1 = File.CreateText(FileName);
            Writer1.Write(Builder.ToString());
            Writer1.Close();
        }

        public void SaveObject(Settings s, string filename)
        {
            XmlSerializer Serializer = new XmlSerializer(s.GetType());
            StringBuilder Builder = new StringBuilder();
            System.Xml.XmlWriterSettings xSettings = new System.Xml.XmlWriterSettings();
            xSettings.Encoding = Encoding.UTF8;
            xSettings.OmitXmlDeclaration = true;
            System.Xml.XmlWriter Writer = System.Xml.XmlWriter.Create(Builder, xSettings);

            using (var outputFile = new FileStream(filename, FileMode.Create))
            {
                using (var compressionStream = new DeflateStream(outputFile, CompressionMode.Compress))
                {
                    Serializer.Serialize(compressionStream, s);
                    Writer.Close();
                    compressionStream.Flush();
                    outputFile.Flush();
                    compressionStream.Close();
                    outputFile.Close();
                }
            }
        }


        public Settings LoadObject(string filename)
        {
            Settings value = new Settings();
            XmlSerializer Serializer = new XmlSerializer(value.GetType());
            StringBuilder Builder = new StringBuilder();

            using (var outputFile = new FileStream(filename, FileMode.Open))
            using (var compressionStream = new System.IO.Compression.DeflateStream(outputFile, System.IO.Compression.CompressionMode.Decompress))
            {
                value = (Settings)Serializer.Deserialize(compressionStream);
                compressionStream.Flush();
                outputFile.Flush();
                compressionStream.Close();
                outputFile.Close();
            }

            return value;
        }



    }
}
