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
        private readonly DownloadManager downloadManager = DownloadManager.Get();
        private string garrysModPath;
        private readonly Uri[] contentDownloadUrls =
        {
            new Uri("https://gmodcontent.de/css-content-gmodcontent.zip"),
            new Uri("https://gmodcontent.de/hl2ep1-content-gmodcontent.zip"),
            new Uri("https://gmodcontent.de/hl2ep2-content-gmodcontent.zip")
        };

        public async void Run()
        {
            Setup();
            
            if (!DetectGmodPath())
                try
                {
                    ManualSetGmodPath();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    return;
                }

            await downloadManager.DownloadList(contentDownloadUrls, Directory.GetCurrentDirectory());

            List<Task> unzipTasks = new List<Task>();
            for (int i = 0; i < contentDownloadUrls.Length; i++)
            {
                string contentPackPath = $@"{ Directory.GetCurrentDirectory() }/{ contentDownloadUrls[i].ToString().Split("/")[contentDownloadUrls[i].ToString().Split("/").Length - 1] }";
                unzipTasks.Add(Task.Run(() => InjectContentPack(contentPackPath)));
            }
            await Task.WhenAll(unzipTasks);
        }

        private void ManualSetGmodPath()
        {
            Console.Write("Please provide Garry's Mod installation path (i.e  C:/Program Files (x86)/Steam/steamapps/common/GarrysMod): ");
            garrysModPath = Console.ReadLine();

            if (!File.Exists(garrysModPath + "/hl2.exe"))
                throw new Exception("Incorrect garrysmod path provided.");
        }

        private void InjectContentPack(string contentPackPath)
        {
            string destination = $@"{garrysModPath}\garrysmod\addons\";
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

        private bool DetectGmodPath()
        {
            string[] possiblePaths =
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\GarrysMod",
                @"D:\Program Files (x86)\Steam\steamapps\common\GarrysMod"
            };
            
            Console.WriteLine("Attempting to auto-detect Garry's Mod installation.");
            
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path + @"\hl2.exe"))
                {
                    Console.WriteLine(@$"Detected Garry's Mod path. { path }\hl2.exe");
                    garrysModPath = path;
                    return true;
                }
            }
            
            return false;
        }

        private void Setup()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "GarrysModTextureFix";
        }
    }
}