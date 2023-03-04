using BTD6_Mod_Manager.IO;
using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using BTD6_Mod_Manager.Lib.MelonMods;
using BTD6_Mod_Manager.Lib.Persistance;
using BTD6_Mod_Manager.Persistance;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        DispatcherTimer blinkTimer;

        public MainWindow()
        {
            InitializeComponent();
            Startup();
        }

        private void Startup()
        {
            SessionData.currentGame = GameType.BTD6;            
            Logger.MessageLogged += Log_MessageLogged;
            blinkTimer = new DispatcherTimer();
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
            var userData = UserData.Instance;

            SessionData.loadedMods = Settings.LoadedSettings.LastUsedMods;

            if (Settings.LoadedSettings.IsNewUser)
            {
                Settings.LoadedSettings.IsNewUser = false;
                Settings.LoadedSettings.Save();

                var diag = MessageBox.Show("Would you like to see a tutorial on how to use this mod manager?", "Open tutorial?", MessageBoxButton.YesNo);
                if (diag == MessageBoxResult.Yes)
                {
                    Process.Start("https://youtu.be/RyB5MyMpOlE?t=613");
                }
                else
                    MessageBox.Show("Okay. If you want to see it later, just click on the \"Help\" at the top of the mod manager," +
                        " then click \"How to use Mod Manager\"");
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

        #region UI Events
        //========================================================

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            blinkTimer.Tick += Console_Timer_Tick;
            blinkTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            Lib.Updaters.ModManager_Updater update = new Lib.Updaters.ModManager_Updater();
            DeleteOldUpdaterFiles();
            var game = GameInfo.GetGame(SessionData.currentGame);
            if (game == null)
            {
                Logger.Log("Failed to get game info!");
                return;
            }

            // check if game directory is found
            if (string.IsNullOrEmpty(game.GameDir) || !Directory.Exists(game.GameDir))
            {
                Logger.Log($"Game directory for {SessionData.currentGame.ToString()} was not found! Please browse for {game.EXEName}", OutputType.MsgBox);
                string path = FileIO.BrowseForFile($"Browse for {SessionData.currentGame.ToString()} EXE", ".exe", "Exe Files (*.exe)|*.exe", Environment.CurrentDirectory);
                if (string.IsNullOrEmpty(path) || !File.Exists(path) || !path.EndsWith(game.EXEName))
                {
                    Logger.Log("The path that was selected was not valid! The program will now close", OutputType.MsgBox);
                    Environment.Exit(0);
                }

                string gameDir = new FileInfo(path).Directory.FullName;
                game.GameDir = gameDir;

                switch (SessionData.currentGame)
                {
                    case GameType.BTD5:
                        UserData.Instance.BTD5Dir = gameDir;
                        break;
                    case GameType.BTDB:
                        UserData.Instance.BTDBDir = gameDir;
                        break;
                    case GameType.BMC:
                        UserData.Instance.BMCDir = gameDir;
                        break;
                    case GameType.BTD6:
                        UserData.Instance.BTD6Dir = gameDir;
                        break;
                    case GameType.BTDAT:
                        UserData.Instance.BTDATDir = gameDir;
                        break;
                    default:
                        break;
                }

                UserData.SaveUserData();


                Logger.Log($"Game Directory was set to {gameDir}", OutputType.MsgBox);
            }

            BgThread.AddToQueue(() =>
            {
                update.HandleUpdates(); 
                string melonLoaderDll = game.GameDir + "\\MelonLoader\\net6\\MelonLoader.dll";
                MelonMod_Handler.HandleUpdates(game.GameDir, melonLoaderDll);
            });

            // check for mod helper and download if not exists
            var files = Directory.GetFiles(game.GameDir + "\\Mods");
            var hasModHelper = files.Any(file => file.ToLower().Replace(" ", "").Replace(".", "").Contains("modhelperdll"));
            if (!hasModHelper)
            {
                var result = MessageBox.Show("It seems you don't have Bloons Mod Helper installed. Most mods require this to work." +
                    " Do you want to download it now?", "Download Bloons Mod Helper?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Great! The download will open when you close this popup. To get it, download BloonsTD6_Mod_Helper.zip and extract it to your BTD6 mod's folder");
                    Process.Start("https://github.com/gurrenm3/BTD-Mod-Helper/releases");
                }
            }
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
            if (finishedLoading)
                return;

            OnFinishedLoading();
            finishedLoading = true;
        }

        private void Main_Closing(object sender, CancelEventArgs e) => Settings.LoadedSettings.Save();

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
            Settings.LoadedSettings.Save();
            Process.Start(Settings.settingsDir);
        }

        //startng herere ====================================================
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(Settings.settingsDir);
            if (!File.Exists(Settings.settingsFilePath))
                Settings.LoadedSettings.Save();

            Process.Start(Settings.settingsFilePath);
        }
        private void Launch_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(SessionData.ModsDir))
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
            Process.Start("https://discord.gg/NnD6nRH");
        }

        private void OpenBTD6_ModDir_Button_Click(object sender, RoutedEventArgs e)
        {
            string dir = SessionData.ModsDir;
            if (String.IsNullOrEmpty(dir))
                return;

            Directory.CreateDirectory(dir);
            Process.Start(dir);
        }

        private void Log_MessageLogged(object sender, Logger.LogEvents e)
        {
            if (e.Output == OutputType.MsgBox)
                MessageBox.Show(e.Message);
            else
            {
                OutputLog.Dispatcher.BeginInvoke((Action)(() =>
                {
                    OutputLog.AppendText(e.Message);
                    OutputLog.ScrollToEnd();
                }));

                if (e.Output == OutputType.Both)
                    System.Windows.Forms.MessageBox.Show(e.Message.Replace(">> ", ""));
            }

            bool showConsoleFlash = Settings.LoadedSettings.ConsoleFlash;
            bool isConsoleCollapsed = OutputLog.Visibility == Visibility.Collapsed;
            if (showConsoleFlash && isConsoleCollapsed)
                blinkTimer.Start();
        }


        private void Test_Button1_Click(object sender, RoutedEventArgs e)
        {
            
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

        private void BrowseModDir_Button_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Press \"Yes\" to try automatically finding game dir, Press \"No\" to browse for it yourself",
                "Auto-Detect Game Dir?", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Cancel)
                return;

            string gameDir = "";
            if (result == MessageBoxResult.Yes)
            {
                gameDir = FileIO.AutoDetectGameDir();
                if (string.IsNullOrEmpty(gameDir))
                {
                    var shouldBrowse = MessageBox.Show("Failed to Auto-Detect game dir, do you want to browse for it manually?",
                        "Browse for game dir?", MessageBoxButton.YesNo);

                    if (shouldBrowse == MessageBoxResult.No)
                        return;
                }
            }

            if (result == MessageBoxResult.No || string.IsNullOrEmpty(gameDir))
                gameDir = FileIO.BrowseForGameDir();

            if (string.IsNullOrEmpty(gameDir))
                return;

            Settings.LoadedSettings.BTD6ModsDir = $"{gameDir}\\Mods";
            Settings.LoadedSettings.Save();
            UserControls.Mods_UserControl.instance.PopulateMods(SessionData.currentGame);
        }

        private void RefreshMods_Button_Click(object sender, RoutedEventArgs e)
        {
            UserControls.Mods_UserControl.instance.PopulateMods(SessionData.currentGame);
        }
    }
}