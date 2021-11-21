using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.IO;
using System.Net.Http;

namespace PodLoad
{
    public partial class FeedAccess
    {
        readonly ParallelOptions maxoptions = new(){MaxDegreeOfParallelism = 3};
        readonly ParallelOptions options = new() { MaxDegreeOfParallelism = 2 };
        readonly HttpClient client;
        private readonly System.Collections.Concurrent.ConcurrentBag<string> errorlist = new();

        public FeedAccess()
        {
            HttpClientHandler handler = new() { UseDefaultCredentials = true, AllowAutoRedirect = true, PreAuthenticate = true };
            client = new HttpClient(handler);
        }
        public void GetLatest(Settings request)
        {
            Parallel.ForEach(request.Items, feed =>
            {
                try { GetLatestItem(feed); }
                catch (Exception ex) { HandleErrors(feed.Id, ex.ToString()); }
            });
        }
        public void DownloadFiles(Settings request)
        {
            Console.WriteLine(GetCount(request).ToString() + " Downloads");
            Parallel.ForEach(request.Items, maxoptions, n => DownloadItems(n));
        }
        public static int GetCount(Settings request) => request.Items.SelectMany(x =>
            x.Download.Where(Y => Y.Downloaded == CustomBoolean.False)).Count();
        public void SaveErrors()
        {
            if (!errorlist.IsEmpty) File.WriteAllText("\\errors.log", string.Join(Environment.NewLine, errorlist));
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
                    var iter = Iterator.Current;
                    var NewFeedItem = new XmlFeedDownload{Downloaded = CustomBoolean.False, Path = iter.ToString()};
                    NewFeedItem.Uid = GenerateId(NewFeedItem.Path);
                    if (IsDistinct(request.Download, NewFeedItem.Uid)) request.Download.Add(NewFeedItem);
                }
            }
            catch (Exception X)
            { HandleErrors(X.ToString()); }
            Console.WriteLine("Feed Completed : " + request.Id);
        }
        private static string GenerateId(string request)
        { return request[(request.LastIndexOf('/') + 1)..].Replace('%', ' '); }
        private static bool IsDistinct(List<XmlFeedDownload> existing, string testobject) =>
            !existing.Where(x => string.Equals(x.Uid, testobject, StringComparison.OrdinalIgnoreCase)).Any();
        #endregion

        #region Methods for Downloads
        private void DownloadItems(XmlFeed request)
        {
            var ItemsToDownload = request.Download.Where(x => x.Downloaded == CustomBoolean.False);
            Parallel.ForEach(ItemsToDownload, options, item => 
                DownloadFile(item, GenerateFileName(request.Id, item.Uid)));
        }
        private static string GenerateFileName(string parentid, string itemid) 
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(dir + parentid)) { Directory.CreateDirectory(dir + parentid); }
            return $"{dir}{parentid}\\{itemid}";     
        }
        private void DownloadFile(XmlFeedDownload request, string filename)
        {
            if (File.Exists(filename)) { File.Delete(filename); }
            Console.WriteLine("Starting:" + request.Uid);
            try
            {
                StandardDownloadFile(request.Path, filename);
                request.Downloaded = CustomBoolean.True;
                Console.WriteLine("Downloaded: " + request.Uid);
            }
            catch (Exception X)
            {
                if (File.Exists(filename)) { File.Delete(filename); }
                HandleErrors(request.Uid, X.Message);
            }
        }
        private void StandardDownloadFile(string url, string savepath)
        {
            using var stream = new FileStream(savepath, FileMode.OpenOrCreate, FileAccess.Write);
            client.GetStreamAsync(url).Result.CopyTo(stream);
        }
        #endregion

        public void HandleErrors(string id, string message) => HandleErrors($"{id}:{message}");
        public void HandleErrors(string Message)
        {
            Console.WriteLine(Message);
            errorlist.Add(Message);
        }
    }
}