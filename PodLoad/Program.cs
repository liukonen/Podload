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

                DataAccess D = new DataAccess();
                Settings S = new Settings();
                if (args.Length == 0)
                {
                    S = D.LoadObject(File);

                    xmlAccess Access = new xmlAccess();
                    Access.GetLatest(S);
                    D.SaveObject(S, File);
                    Access.DownloadFiles(S);

                    D.SaveObject(S, File);
                    Access.SaveErrors();
                }
                else
                {
                    S = D.LoadObject(File);

                    switch (args[0])
                    {
                        case "export":
                            D.saveXml(D.LoadObject(File), FileName);
                            break;
                        case "import":
                            D.SaveObject(D.LoadXML(FileName), File);
                            break;
                        case "add":
                            //Adds a new feed to the list
                            //podload add NewFeedName http://Newfeed/atom.xml
                            xmlFeed newfeed = new xmlFeed();
                            newfeed.download = new List<xmlFeedDownload>();
                            newfeed.id = args[1];
                            newfeed.path = args[2];
                            S.Items.Add(newfeed);
                            D.SaveObject(S, File);

                        break;
                        case "remove":
                            //Removes a feed from the list
                            //podload remove ID
                            S.Items.Remove((from xmlFeed item in S.Items where item.id == args[1] select item).First());
                            D.SaveObject(S, File);
                            break;
                        case "list":
                            //Displays all of the available feeds
                            foreach  (xmlFeed feed in S.Items)
                            {
                                Console.WriteLine(string.Concat(feed.id, " ", feed.path));
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

        private static void DisplayHelp()
        {
            Console.WriteLine("__________________________________________________");
            Console.WriteLine("Welcome to Podload Help:");
            Console.WriteLine("Here is a list of valid commands you can execute under podload...");
            Console.WriteLine("export <takes the zin file and converts it to xml>");
            Console.WriteLine("import <takes the xml file and converts it to a zin file>");
            Console.WriteLine("add <Adds a new feed to the list, example 'add NewFeedName http://Newfeed/atom.xml'>");
            Console.WriteLine("remove <Removes a feed from the list, example 'remove NewFeedName'> ");
            Console.WriteLine("list <displays the names and urls of the current feeds>");
        }
    }
}
