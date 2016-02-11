using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Net;
using System.IO;



public partial class xmlAccess
{
    const int cMaxDegreeOfParallelism = 3;

    public SynchronizedCollection<string> Errorlist = new SynchronizedCollection<string>();

    public void GetLatest(Settings request)
    {
        Parallel.ForEach<xmlFeed>(request.Items, delegate (xmlFeed n)
        {
            try { GetLatestItem(n); }
            catch (Exception X) { HandleErrors(string.Concat(n.id, " : ", X.ToString())); }
        });
    }

    public void DownloadFiles(Settings request)
    {
        Console.Clear();
        Console.WriteLine(GetCount(request).ToString() + " Downloads");
        ParallelOptions options = new ParallelOptions();
        options.MaxDegreeOfParallelism = cMaxDegreeOfParallelism;
        Parallel.ForEach<xmlFeed>(request.Items, options, delegate (xmlFeed n)
        {
            DownloadItems(n);
        });

    }
  
    public int GetCount(Settings request)
    {
        return (from xmlFeed X in request.Items from xmlFeedDownload Y in X.download where Y.downloaded == "False" select Y).ToList().Count;

    }


    public void SaveErrors()
    {
        if (Errorlist.Count > 0)
        {
            StreamWriter A = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\errors.log");
            foreach (string item in Errorlist)
            {
                A.WriteLine(item);
            }
            A.Close();
        }
    }


    #region Private Methods for get latest


    private void GetLatestItem(xmlFeed request)
    {
        XPathExpression DefaultXpath = XPathExpression.Compile("/rss/channel/item/enclosure/@url");
        Console.WriteLine("Getting :" + request.id);
        try
        {
            XPathDocument feed = new XPathDocument(request.path);
            XPathNavigator Navigator = feed.CreateNavigator();
            XPathNodeIterator Iterator;
            Iterator = Navigator.Select(DefaultXpath);
            while (Iterator.MoveNext())
            {
                XPathNavigator Obj = Iterator.Current;
                xmlFeedDownload New = new xmlFeedDownload();
                New.downloaded = "False";
                New.path = Obj.ToString();
                New.uid = GenerateId(New.path);
                if (IsDistinct(request.download, New.uid)) { request.download.Add(New); }
            }
        }
        catch (Exception X)
        { HandleErrors(X.ToString()); }
        Console.WriteLine("Feed Completed : " + request.id);
    }

    private string GenerateId(string request)
    { return request.Substring(request.LastIndexOf('/') + 1).Replace('%', ' '); }

    private bool IsDistinct(List<xmlFeedDownload> existing, string testobject)
    {
        foreach (xmlFeedDownload item in existing)
        {
            if (string.Equals(item.uid, testobject)) { return false; }
        }
        return true;
    }

    #endregion

    #region Methods for Downloads

    private void DownloadItems(object request)
    {
        xmlFeed request1 = (xmlFeed)request;

        var ItemsToDownload = (from xmlFeedDownload Y in request1.download where string.Equals(Y.downloaded, "False", StringComparison.OrdinalIgnoreCase) select Y).ToList();

        ParallelOptions options = new ParallelOptions();
        options.MaxDegreeOfParallelism = 2;

        Parallel.ForEach<xmlFeedDownload>(ItemsToDownload, options, delegate (xmlFeedDownload item)
        {
            if (string.Equals(item.downloaded, "False"))
            {
                string filename = string.Concat(AppDomain.CurrentDomain.BaseDirectory, request1.id, "\\", item.uid);
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + request1.id)) { Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + request1.id); }
                if (File.Exists(filename)) { File.Delete(filename); }
                downloadFile(item, filename);
            }
        });
    }


    private void downloadFile(xmlFeedDownload request, string FileName)
    {
        Console.WriteLine("Starting:" + request.uid);
        try
        {
            string pathToSave = FileName;
            try
            {

                try
                {
                    StandardDownloadFile(request.uid, FileName);
                    request.downloaded = "True";
                    Console.WriteLine("Downloaded: " + request.uid);
                }
                catch //if the standard method failes download, try again using a stream based download
                {
                    StreamDownloadFile(request.path, pathToSave); request.downloaded = "True";
                    Console.WriteLine("Downloaded: " + request.uid);
                }
            }
            catch (Exception X)
            {
                if (File.Exists(pathToSave)) { File.Delete(pathToSave); }
                HandleErrors(string.Concat(request.uid, ":", X.Message));
            }

        }
        catch (Exception X) { HandleErrors(string.Concat(request.uid, ":", X.Message)); }
    }

    private void StandardDownloadFile(string URL, String SavePath)
    {
        using (WebClient Client = new WebClient())
        {
            Client.UseDefaultCredentials = true;
            Client.DownloadFile(new Uri(URL), SavePath);
        }
    }
  
    private void StreamDownloadFile(string URL, String SavePath)
    {
        const int bufferLength = 32768; //32k

        WebRequest wr = WebRequest.CreateDefault(new Uri(URL));
        wr.PreAuthenticate = true;
        wr.UseDefaultCredentials = true;
        int bytesRead = 0;

        using (FileStream writer = new FileStream(SavePath, FileMode.Create))
        {
            using (WebResponse response = wr.GetResponse())
            {
                using (Stream cachedResponse = response.GetResponseStream())
                {
                    byte[] buffer = new byte[bufferLength];
                    do { bytesRead = cachedResponse.Read(buffer, 0, buffer.Length); writer.Write(buffer, 0, bytesRead); } while (bytesRead > 0);
                }
            }

        }
    }

    #endregion

    public void HandleErrors(params string[] Message)
    {
        System.Text.StringBuilder t = new System.Text.StringBuilder();
        foreach (var s in Message)
        { t.Append(s); }
        Console.WriteLine(t.ToString());
        Errorlist.Add(t.ToString());
    }
}
