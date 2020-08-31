using BTD_Backend;
using BTD_Backend.Game;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;

namespace BTD6_Mod_Manager.Classes
{
    class Launcher
    {
        public static void Launch()
        {
            var launcher = new Launcher();

            TempSettings.Instance.SaveSettings();
            BgThread.AddToQueue(() => launcher.LaunchBTD6());
        }

        public void LaunchBTD6()
        {
            BTD6_CrashHandler handler = new BTD6_CrashHandler();
            handler.EnableCrashLog();

            int injectWaitTime = 15000;
            var btd6Info = GameInfo.GetGame(GameType.BTD6);

            string btd6ExePath = SteamUtils.GetGameDir(GameType.BTD6) + "\\" + GameInfo.GetGame(GameType.BTD6).EXEName;
            FileInfo btd6File = new FileInfo(btd6ExePath);

            if (!BTD_Backend.Natives.Windows.IsProgramRunning(btd6File, out Process proc))
                Process.Start("steam://rungameid/" + btd6Info.SteamID);
            else
                injectWaitTime = 0;

            while (!BTD_Backend.Natives.Windows.IsProgramRunning(btd6File, out proc))
                Thread.Sleep(1000);

            if (SessionData.LoadedMods == null)
                return;

            Thread.Sleep(injectWaitTime);

            handler.CheckForCrashes();

            foreach (var modPath in SessionData.LoadedMods)
            {
                if (!File.Exists(modPath))
                {
                    Log.Output("The BTD6 mod  \"" + modPath + "\"  could not be found. Failed to inject it");
                    continue;
                }
                BTD_Backend.Natives.Injector.InjectDll(modPath, proc);
            }
        }
    }
}
