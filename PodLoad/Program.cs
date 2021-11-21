using System;
using System.Collections.Generic;
using System.Linq;

namespace podload
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string FileName = AppDomain.CurrentDomain.BaseDirectory + "podload.xml";
                string File = AppDomain.CurrentDomain.BaseDirectory + "podload.zin";

                var access = new DataAccess();
                var Setting = new Settings();
                if (args.Length == 0)
                {
                    Setting = access.LoadObject(File);

                    XmlAccess xAccess = new XmlAccess();
                    xAccess.GetLatest(Setting);
                    access.SaveObject(Setting, File);
                    xAccess.DownloadFiles(Setting);

                    access.SaveObject(Setting, File);
                    xAccess.SaveErrors();
                }
                else
                {
                    Setting = access.LoadObject(File);

                    switch (args[0])
                    {
                        case "export":
                            access.SaveXml(access.LoadObject(File), FileName);
                            break;
                        case "import":
                            access.SaveObject(access.LoadXML(FileName), File);
                            break;
                        case "add":
                            //Adds a new feed to the list
                            //podload add NewFeedName http://Newfeed/atom.xml
                            XmlFeed newfeed = new XmlFeed
                            {
                                Download = new List<XmlFeedDownload>(),
                                Id = args[1],
                                Path = args[2]
                            };
                            Setting.Items.Add(newfeed);
                            access.SaveObject(Setting, File);

                        break;
                        case "remove":
                            //Removes a feed from the list
                            //podload remove ID
                            Setting.Items.Remove((from XmlFeed item in Setting.Items where item.Id == args[1] select item).First());
                            access.SaveObject(Setting, File);
                            break;
                        case "list":
                            //Displays all of the available feeds
                            foreach  (XmlFeed feed in Setting.Items)
                            {
                                Console.WriteLine(string.Concat(feed.Id, " ", feed.Path));
                            }
                            break;
                        case "-h":
                        case "/?":
                            DisplayHelp();
                                break;
                        default:
                            Console.WriteLine("Not a valid command. Valid commands include the following: ");
                            DisplayHelp();
                            break;
                     }
                }
            }
            catch (Exception X)
            {
                Console.WriteLine(X.ToString());
                Console.Read();
            }
        }

        private static void DisplayHelp() => Console.Write(PodLoad.Properties.Resources.ReadMe);
        
    }
}
