using System;
using System.Collections.Generic;
using System.Linq;

namespace PodLoad
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string xmlfile = AppDomain.CurrentDomain.BaseDirectory + "podload.xml";
                string zinfile = AppDomain.CurrentDomain.BaseDirectory + "podload.zin";
                if (args.Length == 0) { Run(zinfile); }
                else { CheckCondtions(zinfile, xmlfile, args); }
            }
            catch (Exception X)
            {
                Console.WriteLine(X.ToString());
                Console.Read();
            }
        }

        private static void CheckCondtions(string zinfilename, string xmlfilename, string[] args) 
        {
            var setting = DataAccess.LoadObject(zinfilename);
            switch (args[0])
            {
                case "export":
                    DataAccess.SaveXml(DataAccess.LoadObject(zinfilename), xmlfilename);
                    break;
                case "import":
                    DataAccess.SaveObject(DataAccess.LoadXML(xmlfilename), zinfilename);
                    break;
                case "add":
                    AddFeed(setting, args[1], args[2], zinfilename);
                    break;
                case "remove":
                    RemoveFeed(setting, args[1], zinfilename);
                    break;
                case "list":
                    DisplayList(setting);
                    break;
                default:
                    DisplayHelp();
                    break;
            }
        }
        private static void Run(string file) 
        {
            var setting = DataAccess.LoadObject(file);
            var xAccess = new FeedAccess();
            xAccess.GetLatest(setting);
            DataAccess.SaveObject(setting, file);
            xAccess.DownloadFiles(setting);
            DataAccess.SaveObject(setting, file);
            xAccess.SaveErrors();
        }
        private static void AddFeed(Settings setting, string id, string path, string file)
        {
            var newfeed = new XmlFeed { Download = new List<XmlFeedDownload>(), Id = id, Path = path };
            setting.Items.Add(newfeed);
            DataAccess.SaveObject(setting, file);
        }
        private static void RemoveFeed(Settings setting, string feedid, string file)
        {
            setting.Items.Remove(setting.Items.Where(item=> item.Id == feedid).First());
            DataAccess.SaveObject(setting, file);
        }
        private static void DisplayList(Settings setting) 
        {
            foreach (var feed in setting.Items)
                Console.WriteLine($"{feed.Id} {feed.Path}");
        }
        private static void DisplayHelp() => Console.Write(PodLoad.Properties.Resources.ReadMe);
        
    }
}
