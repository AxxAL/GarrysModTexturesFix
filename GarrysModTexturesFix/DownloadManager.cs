using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GarrysModTexturesFix
{
    public class DownloadManager
    {
        private static DownloadManager INSTANCE;
        public static DownloadManager Get()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new DownloadManager();
            }

            return INSTANCE;
        }

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
            webClient.DownloadProgressChanged += (sender, args) => WriteProgressToConsole(args, url);;
            webClient.DownloadFileCompleted += (sender, args) => Console.WriteLine("Download complete!");

            string fileName = url.ToString().Split("/")[url.ToString().Split("/").Length - 1];
            
            try
            {
                Console.WriteLine($"Started download of { url }");
                await webClient.DownloadFileTaskAsync(url, $"{ outputDirectory }/{ fileName }");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Couldn't download " + url + "\n" + exception.Message);
            }
            webClient.Dispose();
        }

        private void WriteProgressToConsole(DownloadProgressChangedEventArgs args, Uri url)
        {
            int progress = args.ProgressPercentage;
            string downloadedMegaBytes = $"{ args.BytesReceived /  1048576}MB/{ args.TotalBytesToReceive / 1048576 }MB";
            Console.WriteLine($"Downloading { url }: { progress }% { downloadedMegaBytes }");
        }
    }
}