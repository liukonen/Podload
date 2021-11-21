using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Net;
using System.IO;
using System.Net.Http;

public partial class XmlAccess
{
    const int cMaxDegreeOfParallelism = 3;
    readonly HttpClient client;

    public SynchronizedCollection<string> Errorlist = new SynchronizedCollection<string>();


    public XmlAccess() {
        HttpClientHandler handler = new HttpClientHandler
        {
            UseDefaultCredentials = true,
            AllowAutoRedirect = true,
            PreAuthenticate = true
        };
        client = new HttpClient(handler);
    }


    public void GetLatest(Settings request)
    {
        Parallel.ForEach(request.Items, n=>
        {
            try { GetLatestItem(n); }
            catch (Exception X) { HandleErrors(n.Id, X.ToString()); }
        });
    }

    public void DownloadFiles(Settings request)
    {
        Console.Clear();
        Console.WriteLine(GetCount(request).ToString() + " Downloads");
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = cMaxDegreeOfParallelism
        };
        Parallel.ForEach(request.Items, options, n=>
        {
            DownloadItems(n);
        });

    }
  
    public int GetCount(Settings request) => request.Items.SelectMany(x => 
        x.Download.Where(Y => string.Equals(Y.Downloaded, "False", StringComparison.OrdinalIgnoreCase))).Count();
    


    public void SaveErrors()
    {
        if (Errorlist.Count > 0) File.WriteAllText("\\errors.log", string.Join(Environment.NewLine, Errorlist));
    }


    #region Private Methods for get latest


    private void GetLatestItem(XmlFeed request)
    {
        XPathExpression DefaultXpath = XPathExpression.Compile("/rss/channel/item/enclosure/@url");
        Console.WriteLine("Getting :" + request.Id);
        try
        {
            var feed = new XPathDocument(request.Path);
            var Iterator = feed.CreateNavigator().Select(DefaultXpath);
            while (Iterator.MoveNext())
            {
                XPathNavigator Obj = Iterator.Current;
                var New = new XmlFeedDownload
                {
                    Downloaded = "False",
                    Path = Obj.ToString()
                };
                New.Uid = GenerateId(New.Path);
                if (IsDistinct(request.Download, New.Uid)) request.Download.Add(New); 
            }
        }
        catch (Exception X)
        { HandleErrors(X.ToString()); }
        Console.WriteLine("Feed Completed : " + request.Id);
    }

    private string GenerateId(string request)
    { return request.Substring(request.LastIndexOf('/') + 1).Replace('%', ' '); }

    private bool IsDistinct(List<XmlFeedDownload> existing, string testobject) =>
        !existing.Where(x => string.Equals(x.Uid, testobject, StringComparison.OrdinalIgnoreCase)).Any();
    

    #endregion

    #region Methods for Downloads

    private void DownloadItems(XmlFeed request)
    {
        var ItemsToDownload = request.Download.Where(x => string.Equals(x.Downloaded, "False", StringComparison.OrdinalIgnoreCase));
    
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2
        };

        Parallel.ForEach(ItemsToDownload, options, item =>
        {
            if (string.Equals(item.Downloaded, "False"))
            {
                var filename = string.Concat(AppDomain.CurrentDomain.BaseDirectory, request.Id, "\\", item.Uid);
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + request.Id)) { Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + request.Id); }
                if (File.Exists(filename)) { File.Delete(filename); }
                DownloadFile(item, filename);
            }
        });
    }


    private void DownloadFile(XmlFeedDownload request, string FileName)
    {
        Console.WriteLine("Starting:" + request.Uid);
        try
        {
            var pathToSave = FileName;
            try
            {

                    StandardDownloadFile(request.Path, FileName);
                    request.Downloaded = "True";
                    Console.WriteLine("Downloaded: " + request.Uid);
                
            }
            catch (Exception X)
            {
                if (File.Exists(pathToSave)) { File.Delete(pathToSave); }
                HandleErrors(request.Uid, X.Message);
            }

        }
        catch (Exception X) { HandleErrors(request.Uid, X.Message); }
    }

    private void StandardDownloadFile(string URL, String SavePath)
    {
        
        using(var stream = new FileStream(SavePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            client.GetStreamAsync(URL).Result.CopyTo(stream);
        }
    }
  

    #endregion

    public void HandleErrors(string id, string message) => HandleErrors($"{id}:{message}");
    public void HandleErrors(string Message)
    {
        Console.WriteLine(Message);
        Errorlist.Add(Message);
    }
}
