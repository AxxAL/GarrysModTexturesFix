using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace GarrysModTextureFixGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string garrysModInstallationPath = string.Empty;
        public int CurrentProgress { get; set; }
        private readonly Uri[] contentDownloadUrls = new Uri[]
        {
            new Uri("https://gmodcontent.de/css-content-gmodcontent.zip"),
            new Uri("https://gmodcontent.de/hl2ep1-content-gmodcontent.zip"),
            new Uri("https://gmodcontent.de/hl2ep2-content-gmodcontent.zip")
        };
        private List<Task> downloadTasks = new List<Task>();

        public MainWindow()
        {
            InitializeComponent();
            DetectGmodPath();
        }

        private async void FixMissingContentButton_Click(object sender, RoutedEventArgs args)
        {
            FixContentButton.IsEnabled = false;
            SelectInstallationPath.IsEnabled = false;
            
            await DownloadListAsync(contentDownloadUrls, Directory.GetCurrentDirectory());
            
            List<Task> unzipTasks = new List<Task>();
            for (int i = 0; i < contentDownloadUrls.Length; i++)
            {
                string contentPackPath = $@"{ Directory.GetCurrentDirectory() }/{ contentDownloadUrls[i].ToString().Split("/")[contentDownloadUrls[i].ToString().Split("/").Length - 1] }";
                unzipTasks.Add(Task.Run(() => InjectContentPack(contentPackPath)));
            }
            await Task.WhenAll(unzipTasks);
            MessageBox.Show("Patched missing textures! Try restarting Garry's Mod.");
            
            FixContentButton.IsEnabled = true;
            SelectInstallationPath.IsEnabled = true;
        }

        private void OpenFileDialog_Click(object sender, RoutedEventArgs args)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() != true) return;
            string path = folderDialog.SelectedPath;

            if (!File.Exists($@"{path}\hl2.exe"))
            {
                MessageBox.Show("Invalid path selected.");
                return;
            }

            SetGmodInstallationPath(path);
        }
        
        private void DetectGmodPath()
        {
            string[] possiblePaths =
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\GarrysMod",
                @"D:\Program Files (x86)\Steam\steamapps\common\GarrysMod",
                @"C:\Games\Steam\steamapps\common\GarrysMod",
                @"D:\Games\Steam\steamapps\common\GarrysMod"
            };

            foreach (string path in possiblePaths)
            {
                if (!File.Exists(@$"{path}\hl2.exe")) continue;
                SetGmodInstallationPath(path);
            }
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
        
        private void SetGmodInstallationPath(string path)
        {
            garrysModInstallationPath = path;
            GmodPathTextBlock.Text = path;
            FixContentButton.IsEnabled = true;
        }

        private async Task DownloadListAsync(Uri[] urls, string outputDirectory)
        {
            foreach (Uri url in urls)
            {
                downloadTasks.Add(DownloadTaskAsync(url, outputDirectory));
            }
            await Task.WhenAll(downloadTasks);
            downloadTasks.Clear();
        }
        
        private async Task DownloadTaskAsync(Uri url, string outputDirectory)
        {
            using WebClient webClient = new WebClient();

            string fileName = url.ToString().Split("/")[url.ToString().Split("/").Length - 1];
            
            webClient.DownloadProgressChanged += (sender, args) => OnDownloadProgressChanged(sender, args, fileName);
            
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

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, string filename)
        {
            switch (filename)
            {
                case "css-content-gmodcontent.zip":
                    CSSDownloadProgress.Value = e.ProgressPercentage;
                    break;
                case "hl2ep1-content-gmodcontent.zip":
                    EP1DownloadProgress.Value = e.ProgressPercentage;
                    break;
                case "hl2ep2-content-gmodcontent.zip":
                    EP2DownloadProgress.Value = e.ProgressPercentage;
                    break;
            }
        }
    }
}