using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.IO;



public partial class xmlAccess
{

    public System.Collections.Generic.SynchronizedCollection<string> Errorlist = new System.Collections.Generic.SynchronizedCollection<string>();

    public void GetLatest(Settings request)
    {
        // equivalent to: foreach (string n in numbers)
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
        options.MaxDegreeOfParallelism = 3;
        Parallel.ForEach<xmlFeed>(request.Items, options, delegate (xmlFeed n)
        {
            DownloadItems(n);
        });

    }
    #region Old unused code
    //List<System.Threading.Thread> ThreadList = new List<System.Threading.Thread>(3);
    //for (int iII = 0; iII < ThreadCount; iII++)
    // {
    //     ThreadList.Add(new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DownloadItems)));
    // }

    // int count = request.Items.Count - 1;
    // int i = 0;


    //while (i != count)
    //{
    //    for (int ii = 0; ii < ThreadCount; ii++)
    //    {
    //        if (ThreadList[ii].ThreadState == System.Threading.ThreadState.Stopped)
    //        {
    //            ThreadList[ii] = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DownloadItems));
    //            ThreadList[ii].Start(request.Items[i]);
    //            i += 1;
    //        }
    //        else if (ThreadList[ii].ThreadState == System.Threading.ThreadState.Unstarted)
    //        {
    //            ThreadList[ii].Start(request.Items[i]);
    //            i += 1;
    //        }
    //        if (i == count) { break; }
    //    }
    //    System.Threading.Thread.Sleep(1000);
    //}

    //old get cout logic
    //  int I = 0;
    //  foreach (xmlFeed feed in request.Items)
    // {
    //     foreach (xmlFeedDownload it in feed.download)
    //     {
    //         if (string.Equals(it.downloaded, "False")) { I += 1; }
    //     }
    // }
    // return I;
    #endregion

    public int GetCount(Settings request)
    {
        return (from xmlFeed X in request.Items from xmlFeedDownload Y in X.download where Y.downloaded == "False" select Y).ToList().Count;

    }


    public void SaveErrors()
    {
        if (Errorlist.Count > 0)
        {
            StreamWriter A = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\" + "errors.log");
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
            // if (string.IsNullOrEmpty(request.xpath) || string.Equals(XpathDefaultString, request.xpath))
            Iterator = Navigator.Select(DefaultXpath);
            //else{XPathExpression Expression = XPathExpression.Compile(request.xpath);
            //Iterator = Navigator.Select(Expression);}
            // XPathNodeIterator Iterator; //= Navigator.Select(Expression);
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
                if (File.Exists(filename)) { File.Delete(filename); }//delete partial files
                downloadFile(item, filename);
            }
        });


        //foreach (xmlFeedDownload item in ItemsToDownload)
        //{

        //    if (string.Equals(item.downloaded, "False"))
        //    {
        //        string filename = string.Concat(AppDomain.CurrentDomain.BaseDirectory, request1.id, "\\", item.uid);
        //        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + request1.id)) { Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + request1.id); }
        //        if (File.Exists(filename)) { File.Delete(filename); }//delete partial files
        //        downloadFile(item, filename);
        //    }
        //}
    }


    // Parallel.ForEach<xmlFeedDownload>(request.download, delegate(xmlFeedDownload item)
    //{
    //    if (string.Equals(item.downloaded, "False")){
    //        string filename = string.Concat(AppDomain.CurrentDomain.BaseDirectory, request.id, "\\", item.uid);
    //        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + request.id)) { Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + request.id); }
    //        if (File.Exists(filename)) { File.Delete(filename); }//delete partial files
    //    downloadFile(item, filename);}
    //});


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
                    WebClient Client = new WebClient();
                    Client.UseDefaultCredentials = true;
                    Client.DownloadFile(new Uri(request.path), pathToSave);
                    Client.Dispose();
                    request.downloaded = "True";
                    Console.WriteLine("Downloaded: " + request.uid);
                }
                catch
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

    public void StreamDownloadFile(string URL, String SavePath)
    {
        const int bufferLength = 32768; //32k

        WebRequest w = WebRequest.CreateDefault(new Uri(URL));
        w.PreAuthenticate = true;
        w.UseDefaultCredentials = true;
        WebResponse response = w.GetResponse();
        int bytesRead = 0;
        FileStream fsWriter = new FileStream(SavePath, FileMode.CreateNew, FileAccess.Write);
        Stream sReader = response.GetResponseStream();
        byte[] buffer = new byte[bufferLength];
        do
        {
            bytesRead = sReader.Read(buffer, 0, buffer.Length);
            fsWriter.Write(buffer, 0, bytesRead);
        }
        while (bytesRead > 0);
        sReader.Close();
        fsWriter.Close();
        response.Close();
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


    /*
           Public Sub HandleErrors(ByVal ParamArray Message() As String)
        Dim t As New Text.StringBuilder()
        For Each s In Message
            t.Append(s)
        Next
        Console.WriteLine(t.ToString)
        Errorlist.Add(t.ToString)
    End Sub
     */
}
