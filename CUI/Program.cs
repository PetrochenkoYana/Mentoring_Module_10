using SiteDownloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUI
{
    class Program
    {
        static void Main(string[] args)
        {
            Downloader downloader = new Downloader(new SiteSaver(new DirectoryInfo("D://SiteDownloader")),3, new List<string>() { "gif","jpeg","jpg","pdf" });
            downloader.DownloadByUrl("https://www.google.by");
        }

    }
}
