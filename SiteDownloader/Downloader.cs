using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SiteDownloader
{
    public class Downloader
    {
        public int DownloadDepth { get; set; }
        public SiteSaver Saver { get; set; }
        public IEnumerable<string> FileTypes { get; set; }
        public Downloader(SiteSaver saver, int downloadDepth, IEnumerable<string> fileTypes)
        {
            Saver = saver;
            DownloadDepth = downloadDepth;
            FileTypes = fileTypes;
        }
        public void DownloadByUrl(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                HandleUrl(httpClient, httpClient.BaseAddress, 0);
            }
        }

        public void HandleUrl(HttpClient client, Uri uri, int currentDepth)
        {
            if (currentDepth > DownloadDepth)
            {
                return;
            }
            HttpResponseMessage head = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)).Result;
            if (head.Content.Headers.ContentType?.MediaType == "text/html")
            {
                ProcessHtmlDocument(client, uri, currentDepth);
            }
            else
            {
                ProcessFile(client, uri);
            }
        }
        private void ProcessFile(HttpClient httpClient, Uri uri)
        {
            var response = httpClient.GetAsync(uri).Result;
            string lastSegment = uri.Segments.Last();
            if (FileTypes.Any(e => lastSegment.EndsWith(e)))
            {
                Saver.SaveFile(uri, response.Content.ReadAsStreamAsync().Result);
            }
        }

        private void ProcessHtmlDocument(HttpClient httpClient, Uri uri, int currentDepth)
        {
            var response = httpClient.GetAsync(uri).Result;
            var document = new HtmlDocument();
            document.Load(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8);
            Saver.SaveHtmlDocument(uri, GetDocumentFileName(document), GetDocumentStream(document));

            var attributesWithLinks = document.DocumentNode.Descendants().SelectMany(d => d.Attributes.Where(attribute => attribute.Name == "src" || attribute.Name == "href"));
            foreach (var attributesWithLink in attributesWithLinks)
            {
                HandleUrl(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), currentDepth + 1);
            }
        }
        private string GetDocumentFileName(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html";
        }
        private Stream GetDocumentStream(HtmlDocument document)
        {
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
