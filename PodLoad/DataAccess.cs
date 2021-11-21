using System;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;

namespace PodLoad
{
    class DataAccess
    {
        public static Settings LoadXML(string fileName)
        {
            if (File.Exists(fileName)) return Load(fileName, false);
            Console.WriteLine("Unable to load file, aborting");
            Console.Beep();
            return null;
        }
        public static Settings LoadObject(string filename) =>
          (File.Exists(filename)) ? Load(filename, true) : CreateNewBlankSettingsFile(filename);
        public static void SaveXml(Settings setting, string filename) { Save(setting, filename, false); }
        public static void SaveObject(Settings setting, string filename) { Save(setting, filename, true); }
        private static Settings CreateNewBlankSettingsFile(string Filename) {
            Console.WriteLine("Unable to find Settings file. Generating new one.");
            var newItem = new Settings();
            SaveObject(newItem, Filename);
            return newItem;
        }
        private static Settings Load(string fileName, bool compressionEnabled)
        {
            var Serializer = new XmlSerializer(typeof(Settings));
            using var outputFile = new FileStream(fileName, FileMode.Open);
            if (!compressionEnabled) return (Settings)Serializer.Deserialize(outputFile);
            using var compressionStream = new DeflateStream(outputFile, CompressionMode.Decompress);
            return (Settings)Serializer.Deserialize(compressionStream);
        }
        private static void Save(Settings setting, string fileName, bool compresionEnabled)
        {
            var Serializer = new XmlSerializer(setting.GetType());
            using var F = new FileStream(fileName, FileMode.Create);
            if (!compresionEnabled) { Serializer.Serialize(F, setting); }
            else
            {
                using var gz = new DeflateStream(F, CompressionMode.Compress, false);
                Serializer.Serialize(gz, setting);
            }
        }
    }
}