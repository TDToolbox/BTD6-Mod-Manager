using BTD6_Mod_Manager.Classes;
using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using BTD6_Mod_Manager.Lib.MelonMods;
using BTD6_Mod_Manager.Lib.Natives;
using BTD6_Mod_Manager.Lib.Persistance;
using Ionic.Zip;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BTD6_Mod_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool doingWork = false;
        public static string workType = "";
        public static MainWindow instance;
        public MainWindow()
        {
            InitializeComponent();
            Logger.MessageLogged += Log_MessageLogged;
            SessionData.CurrentGame = GameType.BTD6;
            Startup();
        }

        private void Startup()
        {
            UserData.UserDataLoaded += UserData_UserDataLoaded;
            TempSettings.Instance.LoadSettings();
            TempSettings.Instance.SaveSettings();

            if (TempSettings.Instance.BTD6_ModsDir.Contains(TempSettings.Instance.MainSettingsDir))
                MoveModsToNewFolder();
        }

        private void OnFinishedLoading()
        {
            SetVersionTextBlock();
            
            Logger.Log("Program initializing...");
            Logger.Log("Welcome to BTD6 Mod Manager!");
            
            string tdloaderDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TD Loader";
            UserData.MainProgramExePath = Environment.CurrentDirectory + "\\BTD6 Mod Manager.exe";
            UserData.MainProgramName = "BTD6 Mod Manager";
            UserData.MainSettingsDir = tdloaderDir;
            UserData.UserDataFilePath = tdloaderDir + "\\userdata.json";

            if (TempSettings.Instance.IsNewUser)
            {
                var diag = MessageBox.Show("Would you like to see a tutorial on how to use this mod manager?", "Open tutorial?", MessageBoxButton.YesNo);
                if (diag == MessageBoxResult.Yes)
                {
                    Windows.WebBrowser browser = new Windows.WebBrowser("How to use this Mod Manager");
                    browser.Show();
                    browser.GoToURL("https://youtu.be/RyB5MyMpOlE?t=613");
                }
                else
                    MessageBox.Show("Okay. If you want to see it later, just click on the \"Help\" at the top of the mod manager," +
                        " then click \"How to use Mod Manager\"");
                TempSettings.Instance.IsNewUser = false;
                TempSettings.Instance.SaveSettings();
            }
        }

        private void SetVersionTextBlock()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] split = version.Split('.');

            if (split.Length - 1 > 2)
                version = version.Remove(version.Length - 2, 2);

            Version_TextBlock.Text = "Version " + version;
        }

        private void MoveModsToNewFolder()
        {
            string oldModsDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TD Loader\\BTD6 Mods";
            
            Logger.Log("Path to BTD6 mods is outdated. Moving mods folder");

            string btd6ExePath = SteamUtils.GetGameDir(SessionData.CurrentGame);
            string newModsDir = btd6ExePath + "\\Mods";

            TempSettings.Instance.BTD6_ModsDir = newModsDir;
            TempSettings.Instance.SaveSettings();

            if (!Directory.Exists(oldModsDir))
                return;

            if (!Directory.Exists(newModsDir))
                Directory.CreateDirectory(newModsDir);

            var files = Directory.GetFiles(oldModsDir);

            foreach (var item in files)
            {
                FileInfo f = new FileInfo(item);
                string newPath = newModsDir + "\\" + f.Name;
                if (!File.Exists(newPath))
                    File.Move(f.FullName, newPath);
            }

            
        }

        private void UserData_UserDataLoaded(object sender, UserData.UserDataEventArgs e)
        {
            /*BTD6_CrashHandler handler = new BTD6_CrashHandler();
            handler.EnableCrashLog();*/
        }

        private void DeleteOldUpdaterFiles()
        {
            var files = new DirectoryInfo(Environment.CurrentDirectory).GetFiles();
            foreach (var file in files)
            {
                if (file.Name == "Updater.exe")
                    file.Delete();

                string cleanedName = file.Name.Replace(".", "").Replace(" ", "").Replace("_", "").ToLower();
                if (cleanedName == "btd6modmanagerzip")
                    file.Delete();
            }
        }

        private void ExtractNewUpdater()
        {
            var files = new DirectoryInfo(Environment.CurrentDirectory).GetFiles();
            foreach (var file in files)
            {
                string cleanedName = file.Name.Replace(".", "").Replace(" ", "").Replace("_", "").ToLower();
                if (cleanedName != "btd6modmanagerzip")
                    continue;

                ZipFile zip = new ZipFile(file.FullName);
                foreach (var entry in zip.Entries)
                {
                    MessageBox.Show(entry.FileName);
                    if (entry.FileName != "Updater.exe")
                        continue;
                    
                    entry.Extract();
                    break;  
                }
            }
        }

        #region UI Events
        //========================================================

        DispatcherTimer blinkTimer = new DispatcherTimer();
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            blinkTimer.Tick += Console_Timer_Tick;
            blinkTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            BTD6_CrashHandler handler = new BTD6_CrashHandler();
            handler.EnableCrashLog();

            Lib.Web.UpdateHandler update = new Lib.Web.UpdateHandler();

            DeleteOldUpdaterFiles();
            ExtractNewUpdater();

            var game = GameInfo.GetGame(SessionData.CurrentGame);
            BgThread.AddToQueue(() => 
            {
                update.HandleUpdates(); //Removed updates due to causing issues
                string gameD = game.GameDir + "\\MelonLoader\\MelonLoader.ModHandler.dll";
                MelonMod_Handler.HandleUpdates(game.GameDir, gameD);
            });
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
                overflowGrid.Visibility = Visibility.Collapsed;

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
                mainPanelBorder.Margin = new Thickness();
        }

        bool finishedLoading = false;
        private void Main_Activated(object sender, EventArgs e)
        {
            if (finishedLoading == false)
            {
                finishedLoading = true;
                OnFinishedLoading();
            }
        }

        private void Main_Closing(object sender, CancelEventArgs e) => TempSettings.Instance.SaveSettings();

        private void ConsoleColapsed(object sender, RoutedEventArgs e)
        {
            if (OutputLog.Visibility == Visibility.Collapsed)
            {
                OutputLog.Visibility = Visibility.Visible;
                CollapseConsole_Button.Content = "Hide Console";
            }
            else
            {
                OutputLog.Visibility = Visibility.Collapsed;
                CollapseConsole_Button.Content = "Show Console";
            }
        }

        private void OpenSettingsDir_Button_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(TempSettings.Instance.MainSettingsDir);
            TempSettings.Instance.SaveSettings();
            Process.Start(TempSettings.Instance.MainSettingsDir);
        }

        //startng herere ====================================================
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(TempSettings.Instance.MainSettingsDir);
            string settingsPath = TempSettings.Instance.MainSettingsDir + "\\" + TempSettings.Instance.settingsFileName;

            if (!File.Exists(settingsPath))
                TempSettings.Instance.SaveSettings();

            Process.Start(settingsPath);
        }
        private void Launch_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TempSettings.Instance.GetModsDir(SessionData.CurrentGame)))
            {
                Logger.Log("Error! You can't launch yet because you need to set a mods directory for your selected game");
                return;
            }

            Launcher.Launch();
        }

        private void Nexus_Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.nexusmods.com/bloonstd6");
        }

        private void Discord_Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/VADMF2M");
        }

        private void OpenBTD6_ModDir_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(TempSettings.Instance.MainSettingsDir))
            {
                Directory.CreateDirectory(TempSettings.Instance.MainSettingsDir);
                TempSettings.Instance.SaveSettings();
                //UserData.SaveUserData();
            }

            string dir = TempSettings.Instance.GetModsDir(SessionData.CurrentGame);
            if (String.IsNullOrEmpty(dir))
                return;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Process.Start(dir);
        }

        private void Log_MessageLogged(object sender, Logger.LogEvents e)
        {
            if (e.Output == OutputType.MsgBox)
                System.Windows.Forms.MessageBox.Show(e.Message);
            else
            {
                OutputLog.Dispatcher.BeginInvoke((Action)(() =>
                {
                    OutputLog.AppendText(e.Message);
                    OutputLog.ScrollToEnd();
                }));
                
                if (e.Output == OutputType.Both)
                    System.Windows.Forms.MessageBox.Show(e.Message.Replace(">> ",""));
            }

            if (TempSettings.Instance.ConsoleFlash && OutputLog.Visibility == Visibility.Collapsed)
                blinkTimer.Start();
        }


        private void Test_Button1_Click(object sender, RoutedEventArgs e)
        {
            /*BTD6_CrashHandler bTD6_CrashHandler = new BTD6_CrashHandler();
            bTD6_CrashHandler.CheckForCrashes();*/
        }

        // The timer's Tick event.
        private bool BlinkOn = false;
        private int blinkCount = 0;
        private void Console_Timer_Tick(object sender, EventArgs e)
        {
            var consoleButtonColor = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
            var consoleDarkButtonColor = new SolidColorBrush(Color.FromArgb(255, 62, 62, 62));

            if (blinkCount >= 6)
            {
                BlinkOn = false;
                blinkCount = 0;
                CollapseConsole_Button.Background = consoleButtonColor;
                CollapseConsole_Button.Foreground = Brushes.Black;
                blinkTimer.Stop();
                return;
            }

            if (BlinkOn)
            {
                CollapseConsole_Button.Foreground = Brushes.Black;
                CollapseConsole_Button.Background = consoleButtonColor;
            }
            else
            {
                CollapseConsole_Button.Background = consoleDarkButtonColor;
                CollapseConsole_Button.Foreground = Brushes.White;
            }

            BlinkOn = !BlinkOn;
            blinkCount++;
        }
        #endregion

        private void HowGetMods_Button_Click(object sender, RoutedEventArgs e)
        {
            Windows.WebBrowser browser = new Windows.WebBrowser("How to get mods");
            browser.Show();
            browser.GoToURL("https://youtu.be/RyB5MyMpOlE?t=1077");
        }
        

        private void HowToUse_Button_Click(object sender, RoutedEventArgs e)
        {
            Windows.WebBrowser browser = new Windows.WebBrowser("How to use this Mod Manager");
            browser.Show();
            browser.GoToURL("https://youtu.be/RyB5MyMpOlE?t=613");
        }

        

        private void ModsBroken_Button_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "\\README.txt"))
                Process.Start(Environment.CurrentDirectory + "\\README.txt");
            else
                Logger.Log("If you are having issues with your mods, you can get help in our discord server! Click the Discord button to join!");
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}