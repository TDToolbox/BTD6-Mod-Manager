using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BTD6_Mod_Manager.Lib.Natives
{
    /// <summary>
    /// Contains Windows related methods, like terminate a process
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// Takes the full name of a resource and loads it in to a stream.
        /// </summary>
        /// <param name="resourceName">Assuming an embedded resource is a file
        /// called info.png and is located in a folder called Resources, it
        /// will be compiled in to the assembly with this fully qualified
        /// name: Full.Assembly.Name.Resources.info.png. That is the string
        /// that you should pass to this method.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(string resourceName)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        public static BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return i;
        }

        /// <summary>
        /// Run a command with command prompt
        /// </summary>
        /// <param name="command">The command to run in the command prompt</param>
        public static void ExecuteCmdCommand(string command)
        {
            Process p = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = @"/c " + command; // cmd.exe spesific implementation
            p.StartInfo = startInfo;
            p.Start();
        }

        /// <summary>
        /// Delete a directory with command prompt
        /// </summary>
        /// <param name="dir">The directory you want to delete</param>
        public static void DeleteDirCMD(string dir) => ExecuteCmdCommand("rmdir /Q /S " + dir.Replace("\\", @"\"));

        /// <summary>
        /// Close process if it's opened
        /// </summary>
        /// <param name="procID">The ID of the process you are trying to close</param>
        public static void KillProcess(int procID)
        {
            var args = new WindowsEventArgs();
            args.ProcessID = procID;
            var process = GetProcess(procID);

            KillProcess(process, args);
        }

        /// <summary>
        /// Close process if it's opened
        /// </summary>
        /// <param name="file">The ID of the process you are trying to close</param>
        public static void KillProcess(FileInfo file)
        {
            var args = new WindowsEventArgs();
            args.File = file;
            var process = GetProcess(file);

            KillProcess(process, args);
        }

        /// <summary>
        /// Close process if it's opened
        /// </summary>
        /// <param name="name">ProcName or WindowTitle of the process to close. 
        /// ProcName is the same as the exe name without the ".exe". WindowTitle is the title of the window</param>
        /// <param name="type">Which part of the process are you looking for, to find and close process</param>
        public static void KillProcess(string name, ProcType type = ProcType.ProcessName)
        {
            var args = new WindowsEventArgs();
            var process = GetProcess(name, type);

            if (type == ProcType.ProcessName)
                args.ProcessName = name;
            else
                args.ProcessWindowTitle = name;

            KillProcess(process, args);
        }

        /// <summary>
        /// Close process if it's opened
        /// </summary>
        /// <param name="process">Process to close</param>
        /// <param name="args">Event args that are called if the process closes successfully or fails</param>
        public static void KillProcess(Process process, WindowsEventArgs args = null)
        {
            if (process == null)
            {
                new Utility().OnFailedToCloseProc(args);
                return;
            }

            process.Kill();
            new Utility().OnProcessClosed(args);
        }

        /// <summary>
        /// Get Process from running processes
        /// </summary>
        /// <param name="procID">The ID of the process you are searching for.</param>
        /// <returns>The process that was found, or null if not found</returns>
        public static Process GetProcess(int procID)
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
                if (process.Id == procID)
                    return process;

            return null;
        }

        /// <summary>
        /// Get Process from running processes
        /// </summary>
        /// <param name="file">The file you want to check to see if running</param>
        /// <returns>The process that was found, or null if not found</returns>
        public static Process GetProcess(FileInfo file)
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
                if (process.ProcessName == file.Name.Replace(".exe", ""))
                    return process;

            return null;
        }

        /// <summary>
        /// Get Process from running processes
        /// </summary>
        /// <param name="type">The part of the process you are searching for, as in ProcessName, ProcessID, WindowTitle.
        /// <param name="name">The name of the process you are searching for.</param>
        /// <returns>The process that was found, or null if not found</returns>
        public static Process GetProcess(string name, ProcType type = ProcType.ProcessName)
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (type == ProcType.ProcessName)
                {
                    if (process.ProcessName == name)
                        return process;
                }
                else if (type == ProcType.WindowTitle)
                {
                    if (process.MainWindowTitle == name)
                        return process;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if a process is running
        /// </summary>
        /// <param name="file">The FileInfo of the file you want to see if running.</param>
        /// <param name="process">The process that was found. Returns null if process is not found</param>
        /// <returns>True or false, whether or not the process is running</returns>
        public static bool IsProgramRunning(FileInfo file, out Process process)
        {
            process = GetProcess(file);
            return process != null;
        }

        /// <summary>
        /// Check if a process is running
        /// </summary>
        /// <param name="procID">The ID of the process you are searching for.</param>
        /// <param name="process">The process that was found. Returns null if process is not found</param>
        /// <returns>True or false, whether or not the process is running</returns>
        public static bool IsProgramRunning(int procID, out Process process)
        {
            process = GetProcess(procID);
            return process != null;
        }

        /// <summary>
        /// Check if a process is running
        /// </summary>
        /// <param name="type">The part of the process you are searching for. 
        /// By ProcessName, ProcessID, and MainWindowTytle</param>
        /// <param name="name">The text of the process you are searching for.</param>
        /// <param name="process">The process that was found. Returns null if process is not found</param>
        /// <returns>True or false, whether or not the process is running</returns>
        public static bool IsProgramRunning(string name, out Process process, ProcType type = ProcType.ProcessName)
        {
            process = GetProcess(name, type);
            return process != null;
        }



        #region Events

        /// <summary>
        /// Event fired when a program is closed
        /// </summary>
        public static event EventHandler<WindowsEventArgs> ProcessClosed;

        /// <summary>
        /// Event fired when a program failed to be closed
        /// </summary>
        public static event EventHandler<WindowsEventArgs> FailedToCloseProc;


        /// <summary>
        /// Events related to WindowsEvents
        /// </summary>
        public class WindowsEventArgs : EventArgs
        {
            /// <summary>
            /// Name of process that was closed or attempted to close
            /// </summary>
            public string ProcessName { get; set; }

            /// <summary>
            /// Window Title for the process you are looking for/attempting to close
            /// </summary>
            public string ProcessWindowTitle { get; set; }

            /// <summary>
            /// Process ID of the process you are looking for/attempting to close
            /// </summary>
            public int ProcessID { get; set; }

            /// <summary>
            /// FileInfo for the file you are looking for/attempting to close
            /// </summary>
            public FileInfo File { get; set; }

        }

        /// <summary>
        /// Fire when a process has been closed
        /// </summary>
        /// <param name="e">args containing the name of the process that was closed</param>
        public void OnProcessClosed(WindowsEventArgs e)
        {
            EventHandler<WindowsEventArgs> handler = ProcessClosed;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Fires when a process failed to close
        /// </summary>
        /// <param name="e">args containing the name of the process that failed to close</param>
        public void OnFailedToCloseProc(WindowsEventArgs e)
        {
            EventHandler<WindowsEventArgs> handler = FailedToCloseProc;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}
