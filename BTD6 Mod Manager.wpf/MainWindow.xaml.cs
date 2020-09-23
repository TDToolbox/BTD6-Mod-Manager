using BTD_Backend;
using BTD_Backend.Game;
using BTD_Backend.Persistence;
using BTD_Backend.Web;
using BTD6_Mod_Manager.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
            if (Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\14.0\VC\Runtimes") != null ||
    Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\DevDiv\vc\Servicing\14.0\RuntimeAdditional") != null) // basically checks for x64 vc redist
            {
                InitializeComponent();
                instance = this;
            }
            else
            {
                MessageBox.Show("You do not have the x64 Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017 and 2019 installed. Clicking OK will bring you to the direct download link. Mods will not work without it.", "Error!");
                Process.Start("https://aka.ms/vs/16/release/vc_redist.x64.exe");
                Environment.Exit(0);
            }

            SessionData.CurrentGame = GameType.BTD6;
            Log.MessageLogged += Log_MessageLogged;

            Log.Output("Program initializing...");
            Startup();
        }

        private void Startup()
        {
            UserData.UserDataLoaded += UserData_UserDataLoaded;
            TempSettings.Instance.LoadSettings();
            TempSettings.Instance.SaveSettings();

            if (TempSettings.Instance.BTD6_ModsDir.Contains(TempSettings.Instance.MainSettingsDir))
                MoveModsToNewFolder();

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] split = version.Split('.');
            
            if (split.Length - 1 > 2)
                version = version.Remove(version.Length - 2, 2);

            Version_TextBlock.Text = "Version " + version;
        }

        private void OnFinishedLoading()
        {
            Log.Output("Welcome to BTD6 Mod Manager!");
            
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

            var game = GameInfo.GetGame(SessionData.CurrentGame);
            string btd6ExePath = game.GameDir + "\\" + game.EXEName;
            FileInfo btd6File = new FileInfo(btd6ExePath);

            BgThread.AddToQueue(() =>
            {
                while (true)
                {
                    if (BTD_Backend.Natives.Utility.IsProgramRunning(btd6File, out Process proc))
                    {
                        Launch_Button.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (var item in TempSettings.Instance.LastUsedMods)
                            {
                                if (item.ToLower().EndsWith(".btd6mod"))
                                {
                                    if (Launch_Button.Content != "Inject")
                                        Launch_Button.Content = "Inject";
                                    break;
                                }
                            }
                        }));
                    }
                    else
                    {
                        Launch_Button.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (Launch_Button.Content != "Launch")
                                Launch_Button.Content = "Launch";
                        }));
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        private void MoveModsToNewFolder()
        {
            string oldModsDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TD Loader\\BTD6 Mods";
            
            Log.Output("Path to BTD6 mods is outdated. Moving mods folder");

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

        #region UI Events
        //========================================================

        DispatcherTimer blinkTimer = new DispatcherTimer();
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            blinkTimer.Tick += Console_Timer_Tick;
            blinkTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            BTD6_CrashHandler handler = new BTD6_CrashHandler();
            handler.EnableCrashLog();
            

            UpdateHandler update = new UpdateHandler()
            {
                GitApiReleasesURL = "https://api.github.com/repos/TDToolbox/BTD6-Mod-Manager/releases",
                ProjectExePath = Environment.CurrentDirectory + "\\BTD6 Mod Manager.exe",
                InstallDirectory = Environment.CurrentDirectory,
                ProjectName = "BTD6 Mod Manager",
                UpdatedZipName = "BTD6_Mod_Manager.zip",
                UpdaterExeName = "Updater.exe"
            };

            var game = GameInfo.GetGame(SessionData.CurrentGame);
            BgThread.AddToQueue(() => 
            {
                update.HandleUpdates();
                string gameD = game.GameDir + "\\MelonLoader\\MelonLoader.ModHandler.dll";
                BTD_Backend.NKHook6.MelonModHandling.HandleUpdates(game.GameDir, gameD);
                
                string nkh = game.GameDir + "\\Mods\\NKHook6.dll";
                BTD_Backend.NKHook6.NKHook6Handler.HandleUpdates(game.GameDir, nkh);
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
            if (!Directory.Exists(TempSettings.Instance.MainSettingsDir))
            {
                Directory.CreateDirectory(TempSettings.Instance.MainSettingsDir);
                TempSettings.Instance.SaveSettings();
                //UserData.SaveUserData();
            }

            Process.Start(TempSettings.Instance.MainSettingsDir);
        }

        //startng herere ====================================================
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            string settingsPath = TempSettings.Instance.MainSettingsDir + "\\" + TempSettings.Instance.settingsFileName;

            if (!File.Exists(settingsPath))
                TempSettings.Instance.SaveSettings();

            Process.Start(settingsPath);
        }
        private void Launch_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TempSettings.Instance.GetModsDir(SessionData.CurrentGame)))
            {
                Log.Output("Error! You can't launch yet because you need to set a mods directory for your selected game");
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

        private void Log_MessageLogged(object sender, Log.LogEvents e)
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
                Log.Output("If you are having issues with your mods, you can get help in our discord server! Click the Discord button to join!");
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}