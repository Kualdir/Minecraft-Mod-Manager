using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;

namespace MinecraftModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string modsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.Minecraft\\mods"; // Gets mods folder;
        private string modmanagerFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.Minecraft\\MinecraftModManager"; // Gets modmanager folder
        public MainWindow()
        {
            InitializeComponent();
            addMod.Foreground = Brushes.ForestGreen;
            removeMod.Foreground = Brushes.Red;
            
            Directory.CreateDirectory(modsFolderPath); // Creates mods folder if it doesn't exist
            Directory.CreateDirectory(modmanagerFolderPath); // Creates modmanager folder if it doesn't exist
            refreshCurrentModsList();

            if (!File.Exists(modmanagerFolderPath + "\\settings.txt")) // if settings txt doesn't exist, run first time setup 
            {
                using (File.Create(modmanagerFolderPath + "\\settings.txt")) { } // Create settings txt, using is used so we can edit the text file later without it being in use
                Directory.CreateDirectory(modmanagerFolderPath + "\\DefaultMods"); // Create defaultmods folder
                Copy(modsFolderPath, modmanagerFolderPath + "\\DefaultMods"); // Copies all files from mods folder to DefaultMods folder

                using (StreamWriter writer = new StreamWriter(modmanagerFolderPath + "\\settings.txt"))
                { 
                    writer.WriteLine("This file is important for the software so please do not delete it.");
                    writer.WriteLine("");
                    writer.WriteLine("You can find the most up to date version at [Insert forum post link]");
                    writer.WriteLine("Software version: 0.1.0");
                    writer.WriteLine("Created by Kualdir");
                    writer.WriteLine("All code can be found on Github at [Github link]");
                }
            }
            if (Directory.Exists(modmanagerFolderPath + "\\Current Mods"))
            {
                Directory.Delete(modmanagerFolderPath + "\\Current Mods", true);
            }
            Directory.CreateDirectory(modmanagerFolderPath + "\\Current Mods"); // Create CurrentModsFolder folder
            Copy(modsFolderPath, modmanagerFolderPath + "\\Current Mods"); // Copies all files from mods folder to CurrentModsFolder folder
            selectedModList.Text = "Current Mods";
            txtChangeListName.Text = "Current Mods";
            refreshModListList();
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        private void refreshCurrentModsList()
        {
            lstCurrentMods.Items.Clear();
            foreach (string file in Directory.EnumerateFiles(modsFolderPath, "*.jar")) // For each item in the mods folder add .jar item to listbox
            {
                lstCurrentMods.Items.Add(file.Remove(0, modsFolderPath.Length + 1));
            }
        }
        private void refreshModListList()
        {
            lstModList.Items.Clear();
            foreach (string folder in Directory.GetDirectories(modmanagerFolderPath))
                lstModList.Items.Add(folder.Remove(0, modsFolderPath.Length + 16));
        }
        private void LoadSubDirs(string dir)
        {
            Console.WriteLine(dir);
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                LoadSubDirs(subdirectory);
            }
        }

        private void addMod_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Jar Files (*.jar)|*.jar";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = true;

            if (choofdlog.ShowDialog() == true)
            {
                string[] allMods = choofdlog.FileNames; //used when Multiselect = true           
                foreach (string modPath in allMods)
                {
                    string sourcePath = modPath;
                    string modName = sourcePath.Split('\\').Last();
                    string targetPath = modsFolderPath + "\\" + modName;
                    File.Copy(sourcePath, modmanagerFolderPath + "\\" + selectedModList.Text.ToString() + "\\" + modName); // Copy mods to mod folder in minecraftmodmanager folder
                    File.Copy(sourcePath, targetPath);
                }
                refreshCurrentModsList();
            }
        }

        private void removeMod_Click(object sender, RoutedEventArgs e)
        {
            if (lstCurrentMods.SelectedIndex == -1) return;
            string selectedMod = lstCurrentMods.SelectedItem.ToString(); // Gets the directory to the mod we want to remove
            try
            {
                File.Delete(modsFolderPath + "\\" + selectedMod); // Deletes the mod out of the mods folder
                File.Delete(modmanagerFolderPath + "\\" + selectedModList.Text.ToString() + "\\" + selectedMod); // Deletes the mod out of the minecraftmodmanager folder
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine(dirNotFound.Message);
            }

            refreshCurrentModsList(); // Refreshes the list so the deleted mod is deleted (while just removing that one selected item would be enough this makes the list accurate again if anything else changed)
        }

        private void openSelectedList_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = lstModList.SelectedItem.ToString();
            selectedModList.Text = selectedFolder;
            txtChangeListName.Text = selectedFolder;
            string path = modsFolderPath;

            DirectoryInfo directory = new DirectoryInfo(path); // Delete mods in mods folder

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }

            Copy(modmanagerFolderPath + "\\" + selectedFolder, modsFolderPath);
            refreshCurrentModsList();
        }
        private void makeNewModList_Click(object sender, RoutedEventArgs e)
        {
            string newFolderName = txtNewModList.Text.ToString();
            if (Directory.Exists(modmanagerFolderPath + "\\" + newFolderName))
            {
                MessageBox.Show("A folder already exists with this name", "Error (#58)", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Directory.CreateDirectory(modmanagerFolderPath + "\\" + newFolderName);
            Copy(modsFolderPath, modmanagerFolderPath + "\\" + newFolderName);
            refreshModListList();
            selectedModList.Text = newFolderName;
        }

        private void makeNewEmptyModList_Click(object sender, RoutedEventArgs e)
        {
            string newFolderName = txtNewModList.Text.ToString();
            if (Directory.Exists(modmanagerFolderPath + "\\" + newFolderName))
            {
                MessageBox.Show("A folder already exists with this name", "Error (#58)", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Directory.CreateDirectory(modmanagerFolderPath + "\\" + newFolderName);
            selectedModList.Text = newFolderName;
            txtChangeListName.Text = newFolderName;
            string path = modsFolderPath;

            DirectoryInfo directory = new DirectoryInfo(path); // Delete mods in mods folder

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
            refreshModListList();
            refreshCurrentModsList();
        }

        private void changeListName_Click(object sender, RoutedEventArgs e)
        {
            string newFolderName = txtChangeListName.Text.ToString();
            if (Directory.Exists(modmanagerFolderPath + "\\" + newFolderName))
            {
                Directory.Delete(modmanagerFolderPath + "\\" + newFolderName, true);
            }
            Directory.CreateDirectory(modmanagerFolderPath + "\\" + newFolderName);
            Directory.Delete(modmanagerFolderPath + "\\" + selectedModList.Text, true);
            selectedModList.Text = newFolderName;
            txtChangeListName.Text = newFolderName;
            Copy(modsFolderPath, modmanagerFolderPath + "\\" + newFolderName);
            refreshModListList();
        }

        private void deleteSelectedList_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = lstModList.SelectedItem.ToString();
            if (selectedFolder != selectedModList.Text.ToString())
            {
                if (Directory.Exists(modmanagerFolderPath + "\\" + selectedFolder))
                {
                    Directory.Delete(modmanagerFolderPath + "\\" + selectedFolder, true);
                }
                refreshModListList();
            }
            else
            {
                MessageBox.Show("Cannot delete open mod list, please open another mod list before deleting this one", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
