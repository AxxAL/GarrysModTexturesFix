using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly Fixer fixer = new Fixer();

        public MainWindow()
        {
            InitializeComponent();
            DetectGmodPath();
        }

        private async void FixMissingContentButton_Click(object sender, RoutedEventArgs args)
        {
            FixContentButton.IsEnabled = false;
            SelectInstallationPath.IsEnabled = false;
            await fixer.Run(garrysModInstallationPath);
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
        
        private void SetGmodInstallationPath(string path)
        {
            garrysModInstallationPath = path;
            GmodPathTextBlock.Text = path;
            FixContentButton.IsEnabled = true;
        }
    }
}