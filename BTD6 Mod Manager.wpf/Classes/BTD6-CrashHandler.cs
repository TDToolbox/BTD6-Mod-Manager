using BTD_Backend;
using BTD_Backend.Game;
using BTD_Backend.Persistence;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BTD6_Mod_Manager.Classes
{
    class BTD6_CrashHandler
    {
        string btd6_bootlog_path = SteamUtils.GetGameDir(GameType.BTD6) + "\\BloonsTD6_Data\\boot.config";
        string crash_report_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp\\Ninja Kiwi\\BloonsTD6\\Crashes\\";

        public void CheckForCrashes()
        {
            BgThread.AddToQueue(() =>
            {
                var checkForCrash_start = DateTime.Now;
                while (BTD_Backend.Natives.Utility.IsProgramRunning(GameInfo.GetGame(GameType.BTD6).ProcName, out Process btd6Proc))
                    Thread.Sleep(100);

                if (!Directory.Exists(crash_report_path))
                    Directory.CreateDirectory(crash_report_path);

                var dirs = Directory.GetDirectories(crash_report_path);
                if (dirs == null)
                    return;

                foreach (var folder in dirs)
                {
                    var info = new DirectoryInfo(folder);
                    if (info.LastWriteTime > checkForCrash_start)
                    {
                        Log.Output("BTD6 has crashed");
                        var result = MessageBox.Show("BTD6 has crashed! Do you want to open the crash log?", "Open crash log?", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                            OpenCrashLog();
                    }
                }
            });
        }

        public void EnableCrashLog()
        {
            if (!File.Exists(btd6_bootlog_path))
            {
                Log.Output("Error! BTD6 boot.config file not found!");
                return;
            }

            string newCrashLog = "";
            var lines = File.ReadAllLines(btd6_bootlog_path);
            if (lines == null)
                return;

            foreach (var line in lines)
            {
                if (!line.Contains("nolog"))
                    newCrashLog += line + "\n";
            }

            File.WriteAllText(btd6_bootlog_path, newCrashLog);
        }

        public void OpenCrashLog()
        {
            if (!Directory.Exists(crash_report_path))
            {
                Log.Output("Error! The crash folder doesn't exist!");
                return;
            }

            string dest = "";
            DateTime mostRecent = new DateTime();
            var files = Directory.GetDirectories(crash_report_path);
            if (files == null)
                return;

            foreach (var item in files)
            {
                var info = new DirectoryInfo(item);

                if (info.LastWriteTime > mostRecent)
                {
                    mostRecent = info.LastWriteTime;
                    dest = info.FullName;
                }
            }
            Process.Start(dest + "\\error.log"); 
        }
    }
}
