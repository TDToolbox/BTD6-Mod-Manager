using BTD_Backend;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;

namespace BTD6_Mod_Manager.Windows
{
    /// <summary>
    /// Interaction logic for DevManager.xaml
    /// </summary>
    public partial class DevManager : Window
    {
        long lastFileSize = 0;
        public bool autoUpdateMod = false;
        //public Thread updateCheck_Thread;
        public string OriginalModPath { get; set; }
        public string CheckUpdateModPath { get; set; }

        private static Dictionary<DevManager, Thread> autoUpdatingMods = new Dictionary<DevManager, Thread>();
        public DevManager()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;            
        }

        public static bool IsModUpdateChecking(string modPath, out DevManager result)
        {
            foreach (var item in autoUpdatingMods.Keys)
            {
                if (item.OriginalModPath != modPath)
                    continue;

                result = item;
                return true;
            }

            result = null;
            return false;
        }

        private void AutoUpdate_MoreInfo_Button_Click(object sender, RoutedEventArgs e)
        {
            string msg = "While this is enabled TD Loader will periodically check your mod to see if " +
                "it's changed. If it has changed, TD Loader will automatically copy the latest version " +
                "to your Mods folder. This can be useful because you won't need to constantly re-import " +
                "your mod during development";

            Log.Output(msg);
        }

        private void CheckState_Changed(object sender, RoutedEventArgs e)
        {
            var enable = AutoUpdate_CB.IsChecked;
            autoUpdateMod = enable.Value;

            if (enable.Value)
            {
                AutoUpdate_TextBox.Visibility = Visibility.Visible;
                AutoUpdate_Browse_Button.Visibility = Visibility.Visible;
                CheckForChanges_Button.Visibility = Visibility.Visible;
            }
            else
            {
                AutoUpdate_TextBox.Visibility = Visibility.Hidden;
                AutoUpdate_Browse_Button.Visibility = Visibility.Hidden;
                CheckForChanges_Button.Visibility = Visibility.Hidden;

                if (IsModUpdateChecking(OriginalModPath, out DevManager mgrInst))
                {
                    autoUpdatingMods[mgrInst].Abort();
                    autoUpdatingMods.Remove(mgrInst);
                }
            }
        }

        private void AutoUpdate_Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            var path = BTD_Backend.IO.FileIO.BrowseForFile("Select the mod you want to check for updates", "", "", "");
            if (string.IsNullOrEmpty(path))
                return;

            AutoUpdate_TextBox.Text = path;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            if (autoUpdateMod && AutoUpdate_TextBox.Text.Length == 0)
            {
                Log.Output("Error! You enabled Auto Updating for this mod, but you haven't selected the path to the " +
                    "file you want to check for changes. You need to chose the location of your mod, the one you're developing.");
                return;
            }
        }

        private void CheckForChanges_Button_Click(object sender, RoutedEventArgs e)
        {
            if (autoUpdateMod && AutoUpdate_TextBox.Text.Length == 0)
            {
                Log.Output("Error! You enabled Auto Updating for this mod, but you haven't selected the path to the " +
                    "file you want to check for changes. You need to chose the location of your mod, the one you're developing.");
                return;
            }

            if (string.IsNullOrEmpty(OriginalModPath))
            {
                Log.Output("Error! Can't check mod for updates because the original mod wasn't set. This is a developer error," +
                    " contact us if you are seeing this message.");
                return;
            }

            CheckUpdateModPath = AutoUpdate_TextBox.Text;
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    if (!File.Exists(CheckUpdateModPath))
                    {
                        Log.Output("Error! File to check for changes doesn't exist!");
                        break;
                    }
                    if (!isFileChanged(CheckUpdateModPath))
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var f = new FileInfo(OriginalModPath);
                    Log.Output("A change was detected in the mod \"" + f.Name + "\". Updating the stored version of this mod to the latest version...");
                    File.Copy(CheckUpdateModPath, OriginalModPath, true);

                    Log.Output("Finished updating \"" + f.Name + "\"");
                }
            });

            t.IsBackground = true;
            if (IsModUpdateChecking(OriginalModPath, out DevManager mgrInst))
            {
                autoUpdatingMods[mgrInst].Abort();
                autoUpdatingMods.Remove(mgrInst);
            }

            mgrInst = this;
            autoUpdatingMods.Add(mgrInst, t);
            autoUpdatingMods[mgrInst].Start();
        }

        
        private bool isFileChanged(string path)
        {
            FileInfo f = new FileInfo(path);
            if (lastFileSize == f.Length)
                return false;

            bool result = true;
            if (lastFileSize == 0)
                result = false;

            lastFileSize = f.Length;
            return result;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsModUpdateChecking(OriginalModPath, out DevManager mgrInst))
            {
                AutoUpdate_TextBox.Visibility = Visibility.Hidden;
                AutoUpdate_Browse_Button.Visibility = Visibility.Hidden;
                CheckForChanges_Button.Visibility = Visibility.Hidden;
            }
            else
            {
                AutoUpdate_CB.IsChecked = true;
                AutoUpdate_TextBox.Text = mgrInst.CheckUpdateModPath;
            }
        }
    }
}
