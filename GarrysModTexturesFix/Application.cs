using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace GarrysModTexturesFix
{
    public class Application
    {
        private string garrysModPath;

        private Uri[] contentDownloadUrls =
        {
            new Uri("http://kajar9.myddns.me/downloads/loccsscontent-OT6d6vwFxj09gRB/CSS_Content_Addon_2021.zip"),
            new Uri("http://kajar9.myddns.me/downloads/lochl2e1content-1unoHYrK5z25D3P/HL2_Ep1_Content_Addon_2021.zip"),
            new Uri("http://kajar9.myddns.me/downloads/lochl2e2content-2I2uG42I4C7G0no/HL2_Ep2_Content_Addon_2021.zip")
        }; // TODO: Major problem. Cannot unzip these archives programatically... "The archive entry was compressed using LZMA and is not supported."

        public async void Run()
        {
            try
            {
                SetGmodPath();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }

            List<Task> downloadTasks = new List<Task>();
            foreach (Uri contentPack in contentDownloadUrls)
            {   
                downloadTasks.Add(Download(contentPack));
            }
            await Task.WhenAll(downloadTasks);

            List<Task> unzipTasks = new List<Task>();
            for (int i = 0; i < contentDownloadUrls.Length; i++)
            {
                string contentPackPath = $@"{ Directory.GetCurrentDirectory() }/{ contentDownloadUrls[i].ToString().Split("/")[contentDownloadUrls[i].ToString().Split("/").Length - 1] }";
                unzipTasks.Add(Task.Run(() => InjectContentPack(contentPackPath)));
            }
            await Task.WhenAll(unzipTasks);
        }

        private void SetGmodPath()
        {
            Console.Write("Please provide Garry's Mod path (i.e  D:/Games/Steam/steamapps/common/GarrysMod): ");
            garrysModPath = Console.ReadLine();

            if (!File.Exists(garrysModPath + "/hl2.exe"))
                throw new Exception("Incorrect garrysmod path provided.");
        }

        private void InjectContentPack(string contentPackPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(contentPackPath, Directory.GetCurrentDirectory());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        
        private async Task Download(Uri url)
        {
            using WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (o, e) => Console.WriteLine($"Downloading {url}: {e.ProgressPercentage}% { e.BytesReceived / 1048576 }MB/{ e.TotalBytesToReceive / 1048576 }MB");
            webClient.DownloadFileCompleted += (o, e) => Console.WriteLine("Download complete!");

            string fileName = url.ToString().Split("/")[url.ToString().Split("/").Length - 1];
            
            try
            {
                await webClient.DownloadFileTaskAsync(url, $"{ Directory.GetCurrentDirectory() }/{ fileName }");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Couldn't download " + url + "\n" + exception.Message);
            }
            webClient.Dispose();
        }
    }
}