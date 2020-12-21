using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace BTD6_Mod_Manager.Lib.IO
{
    /// <summary>
    /// Contains methods relating to files, such as getting file info, adding and exporting files, etc. 
    /// </summary>
    public class FileIO
    {
        /// <summary>
        /// Browse for a file
        /// </summary>
        /// <param name="title">Title of dialog window. Example: "Open game exe"</param>
        /// <param name="defaultExt">Default extension for dialog window. Example: "exe"</param>
        /// <param name="filter">File extension filter for dialog window. Example: "Exe files (*.exe)|*.exe|All files (*.*)|*.*"</param>
        /// <param name="startDir">Starting directory for dialog window. Example: ""   or   "Environment.CurrentDirectory"</param>
        /// <returns></returns>
        public static string BrowseForFile(string title, string defaultExt, string filter, string startDir, bool multiSelect = false)
        {
            OpenFileDialog fileDiag = new OpenFileDialog();
            fileDiag.Title = title;
            fileDiag.DefaultExt = defaultExt;
            fileDiag.Filter = filter;
            fileDiag.Multiselect = multiSelect;
            fileDiag.InitialDirectory = startDir;

            fileDiag.ShowDialog();
            return fileDiag.FileName;
        }

        /// <summary>
        /// Copy a directory and all of its contents, while preserving file structure
        /// </summary>
        /// <param name="source">The directory you want to copy</param>
        /// <param name="dest">Where you want to copy the directory to</param>
        /// <param name="deleteSource">Delete the source directory after copying?</param>
        public static void CopyWholeDir(string source, string dest, bool deleteSource = false)
        {
            if (!Directory.Exists(source))
            {
                Logger.Log("Failed to copy the directory: \"" + source + " \"  to  \"" + dest + "\" . Source doesn't exist");
                return;
            }

            DirectoryInfo destInfo = new DirectoryInfo(dest);
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, dest));

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, dest), true);

            if (deleteSource == true)
                Directory.Delete(source);

            Logger.Log("Copied " + destInfo.Name + "!");
        }

        /// <summary>
        /// Get file version number for exe
        /// </summary>
        /// <param name="file">Fileinfo you want the version number for</param>
        /// <returns></returns>
        public static string GetFileVersion(FileInfo file) => FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;

        /// <summary>
        /// Get file version number for exe
        /// </summary>
        /// <param name="path">the full path to the file</param>
        /// <returns></returns>
        public static string GetFileVersion(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Log("EXE not found! unable to get version for: " + path);
                return "";
            }

            return FileVersionInfo.GetVersionInfo(path).FileVersion;
        }

        /// <summary>
        /// Use this to rename a file to "FileName_Copy 1", and increment as long as there is a copy
        /// </summary>
        /// <param name="path">Path of file to increment</param>
        /// <returns></returns>
        public static string IncrementFileName(string path)
        {
            FileInfo f = new FileInfo(path);
            string filename = f.Name;
            string fileExt = f.Extension;
            string destDir = path.Replace(filename, "");

            int i = 1;
            while (File.Exists(path))
            {
                path = destDir + filename.Replace(fileExt, "") + " - Copy " + i + fileExt;
                i++;
            }
            return path;
        }
    }
}
