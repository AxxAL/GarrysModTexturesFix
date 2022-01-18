using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace GarrysModTextureFixGUI
{
    public class Fixer
    {
        private DownloadManager downloadManager = new DownloadManager();
        private readonly Uri[] contentDownloadUrls = new Uri[]
        {
            new Uri("https://gmodcontent.de/css-content-gmodcontent.zip"),
            new Uri("https://gmodcontent.de/hl2ep1-content-gmodcontent.zip"),
            new Uri("https://gmodcontent.de/hl2ep2-content-gmodcontent.zip")
        };
        private string garrysModInstallationPath = string.Empty;
        
        public async Task Run(string garrysModInstallationPath)
        {
            this.garrysModInstallationPath = garrysModInstallationPath;
            await downloadManager.DownloadList(contentDownloadUrls, Directory.GetCurrentDirectory());
            
            List<Task> unzipTasks = new List<Task>();
            for (int i = 0; i < contentDownloadUrls.Length; i++)
            {
                string contentPackPath = $@"{ Directory.GetCurrentDirectory() }/{ contentDownloadUrls[i].ToString().Split("/")[contentDownloadUrls[i].ToString().Split("/").Length - 1] }";
                unzipTasks.Add(Task.Run(() => InjectContentPack(contentPackPath)));
            }
            await Task.WhenAll(unzipTasks);
            MessageBox.Show("Patched missing textures! Try restarting Garry's Mod.");
        }
        
        private void InjectContentPack(string contentPackPath)
        {
            string destination = $@"{garrysModInstallationPath}\garrysmod\addons\";
            try
            {
                Console.WriteLine($@"Extracting { contentPackPath } to { destination }");
                ZipFile.ExtractToDirectory(contentPackPath, destination);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}