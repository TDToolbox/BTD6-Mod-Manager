using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Classes;
using BTD6_Mod_Manager.UserControls;
using BTD6_Mod_Manager.Windows;
using BTD6_Mod_Manager.Persistance;
using BTD6_Mod_Manager.Lib.Game;

namespace BTD6_Mod_Manager
{
    /// <summary>
    /// Interaction logic for ModItem_UserControl.xaml
    /// </summary>
    public partial class ModItem_UserControl : UserControl
    {
        public string modName = "";
        public string modPath = "";

        public ModItem_UserControl()
        {
            InitializeComponent();
            Mods_UserControl.instance.SelectedMods_ListBox.FontSize = 19;
        }

        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            if (TempGuard.IsDoingWork(MainWindow.workType))
                return;

            bool isBtd6Running = SteamUtils.IsGameRunning(GameType.BTD6);
            if (isBtd6Running)
                Logger.Log("You need to restart BTD6 for your changes to take place", OutputType.Both);

            CheckBox cb = (CheckBox)(sender);

            if (cb.IsChecked == true)
            {
                if (!Settings.LoadedSettings.LoadedFirstMod)
                {
                    Logger.Log("Congrats! You selected your first mod! When you're ready, press the green \"Launch\" button" +
                        " at the top of the program to launch and inject your mods!", OutputType.Both);
                    Settings.LoadedSettings.LoadedFirstMod = true;
                    Settings.LoadedSettings.Save();
                }

                if (!Mods_UserControl.instance.SelectedMods_ListBox.Items.Contains(modName))
                {
                    Mods_UserControl.instance.AddToSelectedModLB(modPath);
                    if (modPath.EndsWith(Mods_UserControl.instance.disabledKey))
                        modPath = modPath.Replace(Mods_UserControl.instance.disabledKey, "");

                    /*Mods_UserControl.instance.SelectedMods_ListBox.Items.Add(modName);
                    Mods_UserControl.instance.SelectedMods_ListBox.SelectedIndex = Mods_UserControl.instance.SelectedMods_ListBox.Items.Count - 1;
                    Mods_UserControl.instance.modPaths.Add(modPath);
                    SessionData.LoadedMods.Add(modPath);
                    Settings.LoadedSettings.Save();*/
                }
            }
            else
            {
                // Is not checked
                if (Mods_UserControl.instance.SelectedMods_ListBox.Items.Contains(modName))
                {
                    Mods_UserControl.instance.RemoveFromSelectedLB(modPath);
                    if (!modPath.EndsWith(Mods_UserControl.instance.disabledKey))
                        modPath += Mods_UserControl.instance.disabledKey;

                    /*int selected = Mods_UserControl.instance.SelectedMods_ListBox.SelectedIndex;
                    Mods_UserControl.instance.SelectedMods_ListBox.Items.Remove(modName);
                    Mods_UserControl.instance.modPaths.Remove(modPath);
                    SessionData.LoadedMods.Remove(modPath);
                    Settings.LoadedSettings.Save();

                    if (selected == 0 && Mods_UserControl.instance.SelectedMods_ListBox.Items.Count >= 1)
                        Mods_UserControl.instance.SelectedMods_ListBox.SelectedIndex = selected;
                    else if (Mods_UserControl.instance.SelectedMods_ListBox.Items.Count > 1)
                        Mods_UserControl.instance.SelectedMods_ListBox.SelectedIndex = selected - 1;*/
                }
            }

            //Mods_UserControl.instance.HandlePriorityButtons();
        }

        public override string ToString() => modPath;

        bool renaming = false;
        RenameWindow renameWindow;
        private void RenameMod_Button_Click(object sender, RoutedEventArgs e)
        {
            renaming = true;
            RenameWindow.RenameComplete += RenameWindow_RenameComplete;
            renameWindow = new RenameWindow();
            renameWindow.Closed += Rename_Closed;
            renameWindow.Show();
        }

        private void Rename_Closed(object sender, EventArgs e)
        {
            if (renaming)
            {
                renaming = false;
                RenameWindow.RenameComplete -= RenameWindow_RenameComplete;
                renameWindow.Closed -= Rename_Closed;
            }
        }

        private void RenameWindow_RenameComplete(object sender, RenameWindow.RenameEventArgs e)
        {
            if (!renaming)
                return;

            string newName = e.NewName;
            FileInfo f = new FileInfo(modPath);
            
            if (!newName.EndsWith(f.Extension))
                newName += f.Extension;

            string dest = f.DirectoryName + "\\" + newName;
            if (File.Exists(dest))
            {
                var result = MessageBox.Show("A file with that name already exists. Do you want to replace it?", "Replace existing file?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    File.Delete(dest);
                else
                    return;
            }

            File.Move(modPath, dest);
            Mods_UserControl.instance.PopulateMods(SessionData.currentGame);
            
            renaming = false;
            RenameWindow.RenameComplete -= RenameWindow_RenameComplete;
            renameWindow.Close();
        }

        private void DeleteMod_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(modPath))
            {
                Mods_UserControl.instance.PopulateMods(SessionData.currentGame);
                return;
            }

            System.Windows.Forms.DialogResult diag = System.Windows.Forms.MessageBox.Show("Are you sure you want to delete this mod?", "Delete mod?", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (diag == System.Windows.Forms.DialogResult.No)
                return;

            File.Delete(modPath);
            Mods_UserControl.instance.PopulateMods(SessionData.currentGame);
        }

        private void OpenMod_Button_Click(object sender, RoutedEventArgs e)
        {
            FileInfo f = new FileInfo(modPath);

            if (!Directory.Exists(f.Directory.FullName))
                return;

            Process.Start(f.Directory.FullName);
        }

        private void OpenDevMgr_Button_Click(object sender, RoutedEventArgs e)
        {
            DevManager mgr = new DevManager();
            mgr.OriginalModPath = modPath;
            mgr.Show();
            More_Button.IsOpen = false;
        }

        private void CopyToClipboard_Button_Click(object sender, RoutedEventArgs e)
        {
            StringCollection paths = new StringCollection();
            paths.Add(modPath);
            Clipboard.SetFileDropList(paths);
            More_Button.IsOpen = false;
        }
    }
}