using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.IO;
using BTD_Backend;
using System.Windows.Forms;

namespace BTD6_Mod_Manager.Classes
{
    class FileIO
    {
        public static bool done;
        public static string BrowseForFile(string title, string defaultExt, string filter, string startDir)
        {
            OpenFileDialog fileDiag = new OpenFileDialog();
            fileDiag.Title = title;
            fileDiag.DefaultExt = defaultExt;
            fileDiag.Filter = filter;
            fileDiag.Multiselect = false;
            fileDiag.InitialDirectory = startDir;

            if (fileDiag.ShowDialog() == DialogResult.OK)
            {
                return fileDiag.FileName;
            }
            else
                return null;
        }
        public static List<string> BrowseForFiles(string title, string defaultExt, string filter, string startDir)
        {
            OpenFileDialog fileDiag = new OpenFileDialog();
            fileDiag.Title = title;
            fileDiag.DefaultExt = defaultExt;
            fileDiag.Filter = filter;
            fileDiag.Multiselect = true;
            fileDiag.InitialDirectory = startDir;

            if (fileDiag.ShowDialog() == DialogResult.OK)
            {
                List<string> files = new List<string>();
                files.AddRange(fileDiag.FileNames);
                return files;
            }
            else
                return null;
        }
        public static string BrowseForDirectory(string title, string startDir)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = title;
            dialog.Multiselect = false;
            dialog.InitialDirectory = startDir;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
                return null;
        }
        public static void CopyDirsAndContents(string source, string destination)
        {
            string[] split = source.Split('\\');
            string dirname = split[split.Length - 1];

            Log.Output("Copying " + dirname + "...");
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
            Log.Output("Copied " + dirname + "!");

            done = true;
        }
    }
}
