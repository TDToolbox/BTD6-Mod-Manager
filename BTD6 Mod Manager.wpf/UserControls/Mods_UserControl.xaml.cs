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
using System.Reflection;
using MelonLoader;
using System.Linq;
using System.Threading;

namespace BTD6_Mod_Manager.UserControls
{
    /// <summary>
    /// Interaction logic for Mods_UserControl.xaml
    /// </summary>
    public partial class Mods_UserControl : UserControl
    {
        public readonly string disabledKey = ".disabled";
        public static Mods_UserControl instance;
        public List<string> modPaths = new List<string>();
        public List<ModItem_UserControl> modItems = new List<ModItem_UserControl>();
        List<string> fileExtensions = new List<string>() { ".dll", ".boo", ".wbp", ".jet", ".zip", ".rar", ".7z", ".btd6mod", ".chai" };

        public Mods_UserControl()
        {
            InitializeComponent();
            RepopulateMods += Mods_UserControl_RepopulateMods;
            instance = this;   
        }

        private void Mods_UserControl_RepopulateMods(object sender, ModUcEventArgs e)
        {
            PopulateMods(SessionData.CurrentGame);
        }

        public void PopulateMods(GameType game)
        {
            //invoking so this can happen on thread
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                string modsDir = TempSettings.Instance.GetModsDir(game);
                if (String.IsNullOrEmpty(modsDir) || !Directory.Exists(modsDir))
                    return;

            
                Mods_ListBox.Items.Clear();
                SelectedMods_ListBox.Items.Clear();
                modPaths = new List<string>();
                modItems = new List<ModItem_UserControl>();

                var mods = new DirectoryInfo(modsDir).GetFiles("*.*");



                foreach (var item in fileExtensions)
                {
                    foreach (var mod in mods)
                    {
                        string modName = mod.Name.Replace(disabledKey, "");
                        if (!modName.EndsWith(item) || Mods_ListBox.Items.Contains(mod) || modName.ToLower().Contains("nkhook"))
                            continue;

                        if (item == ".dll" && !BTD_Backend.NKHook6.MelonModHandling.IsValidMelonMod(mod.FullName))
                            continue;

                        AddItemToModsList(mod);
                    }
                }

                if (TempSettings.Instance.LastUsedMods == null)
                    return;

                List<string> TempList = new List<string>();
                foreach (var mod in TempSettings.Instance.LastUsedMods)
                {
                    if ((!File.Exists(mod) && !File.Exists(mod + disabledKey)) || String.IsNullOrEmpty(mod))
                    {
                        Log.Output("Attempted to add a mod that doesnt exist to the Selected Mods list");
                        continue;
                    }

                    string modName = "";
                    if (File.Exists(mod))
                        modName = mod;
                    else
                        modName = mod + disabledKey;

                    TempList.Add(modName);
                    AddToSelectedModLB(modName);
                }

                if (TempList.Count != TempSettings.Instance.LastUsedMods.Count)
                {
                    TempSettings.Instance.LastUsedMods = TempList;
                    SessionData.LoadedMods = TempList;
                    TempSettings.Instance.SaveSettings();
                }

                SelectedMods_ListBox.SelectedIndex = 0;

            }));
        }

        public void RemoveFromSelectedLB(string modPath)
        {
            var modFile = new FileInfo(modPath);

            SelectedMods_ListBox.Items.Remove(modFile.Name);
            SelectedMods_ListBox.SelectedIndex = SelectedMods_ListBox.Items.Count - 1;

            if (TempSettings.Instance.LastUsedMods.Contains(modFile.FullName))
                TempSettings.Instance.LastUsedMods.Remove(modFile.FullName);

            if (!modFile.FullName.Contains(".btd6mod") && !modFile.FullName.EndsWith(disabledKey))
            {
                string newPath = modFile.FullName + disabledKey;
                if (File.Exists(newPath))
                    File.Delete(newPath);

                File.Move(modFile.FullName, newPath);
            }
        }

        public void AddToSelectedModLB(string modPath)
        {
            if (String.IsNullOrEmpty(modPath.Trim()))
                return;

            FileInfo f = new FileInfo(modPath);
            if (f.FullName.EndsWith(disabledKey))
            {
                string newName = f.FullName.Replace(disabledKey, "");
                if (File.Exists(newName))
                    File.Delete(newName);

                File.Move(f.FullName, newName);
                f = new FileInfo(newName);
            }

            modPaths.Add(f.FullName);
            if (!TempSettings.Instance.LastUsedMods.Contains(f.FullName))
            {
                TempSettings.Instance.LastUsedMods.Add(f.FullName);
                TempSettings.Instance.SaveSettings();
            }

            SelectedMods_ListBox.Items.Add(f.Name);

            foreach (var modItem in modItems)
            {
                if (modItem.ToString().Replace(disabledKey, "") == modPath.Replace(disabledKey, ""))
                    modItem.Enable_CheckBox.IsChecked = true;
            }

            SelectedMods_ListBox.SelectedIndex = SelectedMods_ListBox.Items.Count - 1;
        }
        

        public void AddItemToModsList(string modPath) => AddItemToModsList(new FileInfo(modPath));
        public void AddItemToModsList(FileInfo modFile)
        {
            string modName = "";

            if (!modFile.FullName.Contains(".btd6mod") && !
                TempSettings.Instance.LastUsedMods.Contains(modFile.FullName) && 
                !modFile.FullName.EndsWith(disabledKey))
            {
                string newPath = modFile.FullName + disabledKey;
                if (File.Exists(newPath))
                    File.Delete(newPath);

                File.Move(modFile.FullName, newPath);
                
                modFile = new FileInfo(newPath);
            }

            modName = modFile.Name.Replace(disabledKey, "");

            ModItem_UserControl item = new ModItem_UserControl();
            item.MinWidth = Mods_ListBox.ActualWidth - 31;
            item.ModName.Text = modName;
            item.modName = modName;
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

            TempSettings.Instance.SetModsDir(SessionData.CurrentGame, path);
            Mods_UserControl.instance.PopulateMods(SessionData.CurrentGame);
        }

        #region UI Events
        private void ModsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateMods(SessionData.CurrentGame);

            BgThread.AddToQueue(() =>
            {
                string modsDir = GameInfo.GetGame(SessionData.CurrentGame).GameDir + "\\Mods";
                int lastFileCount = Directory.GetFiles(modsDir).Count();
                
                while (true)
                {
                    Thread.Sleep(1000);
                    var files = Directory.GetFiles(modsDir);
                    if (files.Count() == lastFileCount)
                        continue;

                    lastFileCount = files.Count();
                    OnRepopulateMods(new ModUcEventArgs());
                }
            });
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

            //string allModTypes = "All Mod Types|*.jet;*.zip;*.rar;*.7z;*.btd6mod;*.dll;*.boo;*.chai";
            string allModTypes = "All Mod Types|";
            foreach (var item in fileExtensions)
            {
                allModTypes += "*" + item + ";";
            }
            allModTypes = allModTypes.TrimEnd(';');

            List<string> mods = FileIO.BrowseForFiles("Browse for mods", "", allModTypes + "|Dll files (*.dll)|boo files (*.boo)|wbp files (*.wbp)|Jet files (*.jet)|*.jet|Zip files (*.zip)|*.zip|Rar files (*.rar)|*.rar|7z files (*.7z)|*.7z|BTD6 Mods (*.btd6mod)|*.btd6mod|Chai files (*.chai)", "");

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


        #region Events

        /// <summary>
        /// Event is fired when the password list is successfully aquired
        /// </summary>
        public static event EventHandler<ModUcEventArgs> RepopulateMods;

        /// <summary>
        /// Events related to JetPasswords
        /// </summary>
        public class ModUcEventArgs : EventArgs
        {
            
        }

        /// <summary>
        /// Fired when the password list was successfully aquired. Passes password list as arg
        /// </summary>
        /// <param name="e">JetPasswordEvetnArgs takes the aquired password list as an argument</param>
        public void OnRepopulateMods(ModUcEventArgs e)
        {
            EventHandler<ModUcEventArgs> handler = RepopulateMods;
            if (handler != null)
                handler(this, e);
        }

        #endregion



        #endregion
    }
}
