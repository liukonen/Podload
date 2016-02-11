using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    if (args[0] == "export")
                    {
                        S = D.LoadObject(File);
                        D.saveXml(S, FileName);
                    }
                    if (args[0] == "import")
                    {
                        S = D.LoadXML(FileName);
                        D.SaveObject(S, File);

                    }
                }
            }
            catch (Exception X)
            {
                Console.WriteLine(X.ToString());
                Console.Read();

            }
        }



    }
}
