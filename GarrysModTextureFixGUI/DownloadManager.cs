using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace GarrysModTextureFixGUI
{
    public class DownloadManager
    {
        public int CurrentProgress { get; set; }
        
        public async Task DownloadList(Uri[] urls, string outputDirectory)
        {
            List<Task> downloadTasks = new List<Task>();
            foreach (Uri url in urls)
            {
                downloadTasks.Add(DownloadTask(url, outputDirectory));
            }
            await Task.WhenAll(downloadTasks);
        }
        
        private async Task DownloadTask(Uri url, string outputDirectory)
        {
            using WebClient webClient = new WebClient();
            
            string fileName = url.ToString().Split("/")[url.ToString().Split("/").Length - 1];
            
            try
            {
                await webClient.DownloadFileTaskAsync(url, $"{ outputDirectory }/{ fileName }");
            }
            catch (Exception exception)
            {
                MessageBox.Show("Couldn't download " + url + "\n" + exception.Message);
            }
            webClient.Dispose();
        }
    }
}