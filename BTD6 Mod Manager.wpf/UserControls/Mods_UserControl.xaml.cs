using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BTD6_Mod_Manager.Classes;
using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using BTD6_Mod_Manager.Persistance;

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
        List<string> fileExtensions = new List<string>() { ".dll", ".jet", ".zip", ".rar", ".7z", ".btd6mod"};

        public Mods_UserControl()
        {
            InitializeComponent();
            RepopulateMods += Mods_UserControl_RepopulateMods;
            instance = this;   
        }

        private void Mods_UserControl_RepopulateMods(object sender, ModUcEventArgs e)
        {
            PopulateMods(SessionData.currentGame);
        }

        public void PopulateMods(GameType game)
        {
            //invoking so this can happen on thread
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                Mods_ListBox.Items.Clear();
                SelectedMods_ListBox.Items.Clear();
                modPaths = new List<string>();
                modItems = new List<ModItem_UserControl>();

                var mods = new DirectoryInfo(SessionData.ModsDir).GetFiles("*.*");



                foreach (var item in fileExtensions)
                {
                    foreach (var mod in mods)
                    {
                        string modName = mod.Name.Replace(disabledKey, "");
                        if (!modName.EndsWith(item) || Mods_ListBox.Items.Contains(mod))
                            continue;

                        AddItemToModsList(mod);
                    }
                }

                if (Settings.LoadedSettings.LastUsedMods == null)
                    return;

                List<string> TempList = new List<string>();
                foreach (var mod in Settings.LoadedSettings.LastUsedMods)
                {
                    if ((!File.Exists(mod) && !File.Exists(mod + disabledKey)) || String.IsNullOrEmpty(mod))
                    {
                        Logger.Log("Attempted to add a mod that doesnt exist to the Selected Mods list");
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

                if (TempList.Count != Settings.LoadedSettings.LastUsedMods.Count)
                {
                    Settings.LoadedSettings.LastUsedMods = TempList;
                    SessionData.loadedMods = TempList;
                    Settings.LoadedSettings.Save();
                }

                SelectedMods_ListBox.SelectedIndex = 0;

            }));
        }

        public void RemoveFromSelectedLB(string modPath)
        {
            var modFile = new FileInfo(modPath);

            SelectedMods_ListBox.Items.Remove(modFile.Name);
            SelectedMods_ListBox.SelectedIndex = SelectedMods_ListBox.Items.Count - 1;

            if (Settings.LoadedSettings.LastUsedMods.Contains(modFile.FullName))
                Settings.LoadedSettings.LastUsedMods.Remove(modFile.FullName);

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
            if (!Settings.LoadedSettings.LastUsedMods.Contains(f.FullName))
            {
                Settings.LoadedSettings.LastUsedMods.Add(f.FullName);
                Settings.LoadedSettings.Save();
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
            bool isBtdApiMod = modFile.FullName.Contains(".btd6mod");
            bool isLastUsedMod = Settings.LoadedSettings.LastUsedMods.Contains(modFile.FullName);
            bool isDisabled = modFile.FullName.EndsWith(disabledKey);

            if (!isBtdApiMod && !isLastUsedMod && !isDisabled)
            {
                string newPath = modFile.FullName + disabledKey;
                if (File.Exists(newPath))
                    File.Delete(newPath);

                File.Move(modFile.FullName, newPath);
                modFile = new FileInfo(newPath);
            }

            if (isBtdApiMod && !Settings.LoadedSettings.ShownBtdApiInjectorMessage)
            {
                Logger.Log("One or more of your mods are BTD API mods. This means you need to use an injector to inject them into BTD6.", OutputType.Both);

                string btd6ModsDir = SessionData.ModsDir;
                if (!File.Exists(btd6ModsDir + "\\BtdAPI_Injector.dll") && !File.Exists(btd6ModsDir + "\\BtdAPI_Injector.dll.disabled"))
                {
                    var result = MessageBox.Show("The Injector for BTD6 API mods was not found. It is required in order to use BTD6 API mods." +
                        " Do you want to download it?", "Download BTD6 API Injector?", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("https://www.nexusmods.com/bloonstd6/mods/41?tab=description");
                    }
                    else
                    {
                        Logger.Log("You chose not to download the injector. You're BTD6 API mods won't work until you get it" +
                            ". However, all other BTD6 mods will continue to work.", OutputType.Both);
                    }
                }
                Settings.LoadedSettings.ShownBtdApiInjectorMessage = true;
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

        #region UI Events
        private void ModsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateMods(SessionData.currentGame);

            /*BgThread.AddToQueue(() =>
            {
                int lastFileCount = Directory.GetFiles(SessionData.ModsDir).Count();

                while (true)
                {
                    Thread.Sleep(1000);
                    var files = Directory.GetFiles(SessionData.ModsDir);
                    if (files.Count() == lastFileCount)
                        continue;

                    lastFileCount = files.Count();
                    OnRepopulateMods(new ModUcEventArgs());
                }
            });*/
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
            string modFolder = SessionData.ModsDir;
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

            List<string> mods = FileIO.BrowseForFiles("Browse for mods", "", allModTypes + "|Dll files (*.dll)|Jet files (*.jet)|*.jet|Zip files (*.zip)|*.zip|Rar files (*.rar)|*.rar|7z files (*.7z)|*.7z|BTD6API mods (*.btd6mod)|*.btd6mod|", "");

            if (mods == null || mods.Count == 0)
            {
                MainWindow.doingWork = false;
                return;
            }

            foreach (string mod in mods)
            {
                FileInfo f = new FileInfo(mod);
                Logger.Log("Added " + f.Name);

                string dest = Lib.IO.FileIO.IncrementFileName(modFolder + "\\" + f.Name);
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
