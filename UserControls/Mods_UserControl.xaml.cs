using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BTD6_Mod_Manager.Classes;
using BTD_Backend;
using BTD_Backend.Game;
using System.Resources;
using System.Windows.Threading;
using System.Windows.Media;
using System.Diagnostics;

namespace BTD6_Mod_Manager.UserControls
{
    /// <summary>
    /// Interaction logic for Mods_UserControl.xaml
    /// </summary>
    public partial class Mods_UserControl : UserControl
    {
        public static Mods_UserControl instance;
        public List<string> modPaths = new List<string>();
        public List<ModItem_UserControl> modItems = new List<ModItem_UserControl>();
        public Mods_UserControl()
        {
            InitializeComponent();
            instance = this;   
        }

        public void PopulateMods(GameType game)
        {
            string modsDir = TempSettings.Instance.GetModsDir(game);
            if (String.IsNullOrEmpty(modsDir) || !Directory.Exists(modsDir))
                return;

            Mods_ListBox.Items.Clear();
            SelectedMods_ListBox.Items.Clear();
            modPaths = new List<string>();
            modItems = new List<ModItem_UserControl>();

            var mods = new DirectoryInfo(modsDir).GetFiles("*.*");
            List<string> fileExtensions = new List<string>() { ".jet", ".zip", ".rar", ".7z", ".btd6mod" };

            foreach (var mod in mods)
            {
                bool goodExtension = false;
                foreach (var item in fileExtensions)
                {
                    if (!mod.Name.EndsWith(item))
                        continue;
                    
                    goodExtension = true;
                    break;
                }

                if (!goodExtension || Mods_ListBox.Items.Contains(mod))
                    continue;

                AddItemToModsList(mod);
            }

            if (TempSettings.Instance.LastUsedMods == null)
                return;

            List<string> TempList = new List<string>();
            foreach (var selected in TempSettings.Instance.LastUsedMods)
            {
                if (!File.Exists(selected) || String.IsNullOrEmpty(selected))
                {
                    Log.Output("Attempted to add a mod that doesnt exist to the Selected Mods list");
                    continue;
                }
                TempList.Add(selected);
                AddToSelectedModLB(selected);
            }

            if (TempList.Count != TempSettings.Instance.LastUsedMods.Count)
            {
                TempSettings.Instance.LastUsedMods = new List<string>();
                foreach (var item in TempList)
                {
                    TempSettings.Instance.LastUsedMods.Add(item);
                }
            }

            SelectedMods_ListBox.SelectedIndex = 0;
        }

        private void AddToSelectedModLB(string modPath)
        {
            FileInfo f = new FileInfo(modPath);

            modPaths.Add(modPath);
            SelectedMods_ListBox.Items.Add(f.Name);

            foreach (var modItem in modItems)
            {
                if (modItem.ToString() == modPath)
                    modItem.Enable_CheckBox.IsChecked = true;
            }

            SelectedMods_ListBox.SelectedIndex = SelectedMods_ListBox.Items.Count - 1;
        }
        

        public void AddItemToModsList(string modPath) => AddItemToModsList(new FileInfo(modPath));
        public void AddItemToModsList(FileInfo modFile)
        {
            /*if (Mods_ListBox.ActualWidth <= 0)
                return;*/

            ModItem_UserControl item = new ModItem_UserControl();
            item.MinWidth = Mods_ListBox.ActualWidth - 31;
            item.ModName.Text = modFile.Name;
            item.modName = modFile.Name;
            item.modPath = modFile.FullName;

            Thickness margin = item.Margin;
            if (Mods_ListBox.Items.Count == 0)
            {
                margin.Top = 10;
                item.Margin = margin;
            }
            
            Mods_ListBox.Items.Add(item);
            modItems.Add(item);
        }

        public void SetModsDir()
        {
            if (SessionData.CurrentGame == GameType.None)
                return;

            string path = FileIO.BrowseForDirectory("Choose a directory for your mods", Environment.CurrentDirectory);

            if (String.IsNullOrEmpty(path))
                return;

            var gameTypeList = new List<GameType>() { GameType.BTD6, GameType.BTD5, GameType.BTDB, GameType.BMC, GameType.BTDAT, GameType.NKArchive };
            foreach (var item in gameTypeList)
            {
                if (TempSettings.Instance.GetModsDir(item) == path && SessionData.CurrentGame != item)
                {
                    Log.Output("Error! Can't use this path. The location you chose is being used by " + item.ToString()
                        + ". Please use another path for your mods folder");
                    return;
                }
            }

            //Mods_Dir_TextBox.Text = path;
            TempSettings.Instance.SetModsDir(SessionData.CurrentGame, path);
            Mods_UserControl.instance.PopulateMods(SessionData.CurrentGame);
        }

        #region UI Events
        private void ModsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateMods(SessionData.CurrentGame);
        }
        private void ModsUserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (ModItem_UserControl item in Mods_ListBox.Items)
            {
                if ((Mods_ListBox.ActualWidth - 31) > 0)
                    item.MinWidth = Mods_ListBox.ActualWidth - 31;
            }
        }


        private void AddMods_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TempSettings.Instance.GetModsDir(SessionData.CurrentGame)))
            {
                SetModsDir();
                if (String.IsNullOrEmpty(TempSettings.Instance.GetModsDir(SessionData.CurrentGame)))
                {
                    Log.Output("Can't add mods. You need to set a mods directory.");
                    return;
                }
            }

            string modFolder = TempSettings.Instance.GetModsDir(SessionData.CurrentGame);
            if (!Directory.Exists(modFolder))
                Directory.CreateDirectory(modFolder);

            MainWindow.doingWork = true;
            MainWindow.workType = "Adding mods";

            string allModTypes = "All Mod Types|*.jet;*.zip;*.rar;*.7z;*.btd6mod";
            List<string> mods = FileIO.BrowseForFiles("Browse for mods", "", allModTypes + "|Jet files (*.jet)|*.jet|Zip files (*.zip)|*.zip|Rar files (*.rar)|*.rar|7z files (*.7z)|*.7z|BTD6 Mods (*.btd6mod)|*.btd6mod", "");

            if (mods == null || mods.Count == 0)
            {
                MainWindow.doingWork = false;
                return;
            }

            foreach (string mod in mods)
            {
                FileInfo f = new FileInfo(mod);
                Log.Output("Added " + f.Name);

                string dest = BTD_Backend.IO.FileIO.IncrementFileName(modFolder + "\\" + f.Name);
                File.Copy(mod, dest);
                f = new FileInfo(dest);
                
                AddItemToModsList(f.FullName);
            }

            MainWindow.doingWork = false;
            MainWindow.workType = "";
        }

        //private void SelectedMods_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => HandlePriorityButtons();

        
        #endregion
    }
}
