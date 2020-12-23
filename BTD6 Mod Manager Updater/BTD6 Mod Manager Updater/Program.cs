using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace BTD6_Mod_Manager_Updater
{
    class Program
    {
        static string projName = "BTD6 Mod Manager";
        static void Main(string[] args)
        {
            Output("Welcome to the updater for " + projName);
            if (args.Length == 0)
                return;

            Output("Updater for " + projName + " has started!");
            CloseWindow(projName);
            CloseWindow(projName);  //Try one more time just to be safe and make sure it's closed
            ExtractFiles();

            string projExe = Environment.CurrentDirectory + "\\BTD6 Mod Manager.exe";
            if (!File.Exists(projExe))
                return;

            Output("Restarting " + projName + "...");
            Process.Start(projExe);
            Output("Update complete. You may now close this window");
        }

        public static void ExtractFiles()
        {
            Output("Extracting files from Update");
            var files = Directory.GetFiles(Environment.CurrentDirectory);
            foreach (var file in files)
            {
                if (!file.EndsWith(".zip") && !file.EndsWith(".rar") && !file.EndsWith(".7z"))
                    continue;

                var fileInfo = new FileInfo(file);
                string cleanedName = fileInfo.Name.Replace(".", "").Replace(" ","").Replace("_","").ToLower();
                if (cleanedName != "btd6modmanagerzip")
                    continue;

                

                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, entry.FullName));
                        if (destinationPath.StartsWith(Environment.CurrentDirectory, StringComparison.Ordinal))
                        {
                            if (new FileInfo(destinationPath).Name == "Updater.exe")
                                continue;

                            if (File.Exists(destinationPath))
                                File.Delete(destinationPath);

                            entry.ExtractToFile(destinationPath);
                            Output($"Extracting file: {destinationPath}");
                        }
                    }
                }
            }
        }

        public static void CloseWindow(string windowMainTitle)
        {
            while (IsProgramRunning(windowMainTitle))
            {
                var openWindowProcesses = Process.GetProcesses()
        .Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer");

                foreach (var a in openWindowProcesses)
                {
                    //if (a.MainWindowTitle == windowMainTitle)
                    if (a.MainWindowTitle == windowMainTitle)
                    {
                        Output(projName + " is currently open. Close it to continue update");
                        a.CloseMainWindow();
                    }
                }

                Thread.Sleep(350);
            }
        }

        public static bool IsProgramRunning(string windowMainTitle)
        {
            var openWindowProcesses = System.Diagnostics.Process.GetProcesses()
        .Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer");

            foreach (var a in openWindowProcesses)
            {
                if (a.MainWindowTitle == windowMainTitle)
                    return true;
            }
            return false;
        }

        public static void Output(string text)
        {
            Console.WriteLine(">> " + text);
        }
    }
}
