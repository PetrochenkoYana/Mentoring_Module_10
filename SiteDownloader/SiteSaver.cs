using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteDownloader
{
    public class SiteSaver
    {
        public DirectoryInfo RootDirectory { get; set; }

        public SiteSaver(DirectoryInfo root)
        {
            RootDirectory = root;
        }

        public void SaveHtmlDocument(Uri uri, string name, Stream documentStream)
        {
            string directoryPath = Path.Combine(RootDirectory.FullName, uri.Host) + uri.LocalPath.Replace("/", @"\");
            Directory.CreateDirectory(directoryPath);
            var invalidSymbols = Path.GetInvalidFileNameChars();
            name = new string(name.Where(c => !invalidSymbols.Contains(c)).ToArray());
            string fileFullPath = Path.Combine(directoryPath, name);
            SaveToFile(documentStream, fileFullPath);
            documentStream.Close();
        }

        public void SaveFile(Uri uri, Stream fileStream)
        {
            string fileFullPath = Path.Combine(RootDirectory.FullName, uri.Host) + uri.LocalPath.Replace("/", @"\");
            var directoryPath = Path.GetDirectoryName(fileFullPath);
            Directory.CreateDirectory(directoryPath);
            if (Directory.Exists(fileFullPath))
            {
                fileFullPath = Path.Combine(fileFullPath, Guid.NewGuid().ToString());
            }

            SaveToFile(fileStream, fileFullPath);
            fileStream.Close();
        }

        private void SaveToFile(Stream stream, string fileFullPath)
        {
            var createdFileStream = File.Create(fileFullPath);
            stream.CopyTo(createdFileStream);
            createdFileStream.Close();
        }
    }
}
