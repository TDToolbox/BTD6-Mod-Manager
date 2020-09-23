using BTD_Backend;
using BTD_Backend.Game;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace BTD6_Mod_Manager.Classes
{
    class Launcher
    {
        public static void Launch()
        {
            var launcher = new Launcher();

            TempSettings.Instance.SaveSettings();

            bool inject = false;
            foreach (var item in TempSettings.Instance.LastUsedMods)
            {
                if (item.ToLower().EndsWith(".btd6mod"))
                {
                    inject = true;
                    break;
                }
            }
            
            if (inject)
                BgThread.AddToQueue(() => launcher.LaunchBTD6());
            else
            {
                var gameInfo = GameInfo.GetGame(SessionData.CurrentGame);

                if (!BTD_Backend.Natives.Utility.IsProgramRunning(gameInfo.ProcName, out var btd6Proc))
                    Process.Start("steam://rungameid/" + gameInfo.SteamID);
            }
        }

        public void LaunchBTD6()
        {
            BTD6_CrashHandler handler = new BTD6_CrashHandler();
            handler.EnableCrashLog();

            int injectWaitTime = WaitForBTD6(out Process btd6Proc);

            if (SessionData.LoadedMods == null)
                return;

            HandleInjectionTimer(injectWaitTime);

            handler.CheckForCrashes();

            InjectMods(btd6Proc);
        }

        private int WaitForBTD6(out Process btd6Proc)
        {
            int injectWaitTime = 15000;
            var gameInfo = GameInfo.GetGame(SessionData.CurrentGame);

            if (!BTD_Backend.Natives.Utility.IsProgramRunning(gameInfo.ProcName, out btd6Proc))
                Process.Start("steam://rungameid/" + gameInfo.SteamID);
            else
                injectWaitTime = 0;

            while (!BTD_Backend.Natives.Utility.IsProgramRunning(gameInfo.ProcName, out btd6Proc))
                Thread.Sleep(1000);

            return injectWaitTime;
        }

        private void HandleInjectionTimer(int injectWaitTime)
        {
            while (injectWaitTime > 0)
            {
                MainWindow.instance.Timer_TextBlock.Dispatcher.BeginInvoke((System.Action)(() =>
                {
                    MainWindow.instance.Timer_TextBlock.Visibility = Visibility.Visible;

                    var timeLeft = injectWaitTime / 1000;
                    if (timeLeft == 1)
                        MainWindow.instance.Timer_TextBlock.Text = "Time to inject: " + timeLeft + " second";
                    else
                        MainWindow.instance.Timer_TextBlock.Text = "Time to inject: " + timeLeft + " seconds";
                }));

                Thread.Sleep(1000);
                injectWaitTime -= 1000;

                if (injectWaitTime <= 0)
                {
                    MainWindow.instance.Timer_TextBlock.Dispatcher.BeginInvoke((System.Action)(() =>
                    {
                        MainWindow.instance.Timer_TextBlock.Visibility = Visibility.Collapsed;
                    }));
                }
            }
        }

        private void InjectMods(Process btd6Proc)
        {
            bool modsInjected = false;
            Log.Output("Injecting mods...");
            foreach (var modPath in SessionData.LoadedMods)
            {
                if (!File.Exists(modPath))
                {
                    Log.Output("The BTD6 mod  \"" + modPath + "\"  could not be found. Failed to inject it");
                    continue;
                }
                BTD_Backend.Natives.Injector.InjectDll(modPath, btd6Proc);
                modsInjected = true;
            }

            if (modsInjected)
                Log.Output("Mods Injected...");
        }
    }
}
