using BTD6_Mod_Manager.Lib.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace BTD6_Mod_Manager.Lib.Updaters
{
    /// <summary>
    /// Contains methods relating to checking for, getting, and installing updates
    /// </summary>
    public class Universal_Updater
    {
        #region Properties
        /// <summary>
        /// Should the downloaded zip file be deleted after extraction?
        /// </summary>
        public bool DeleteDownloadZip { get; set; }

        /// <summary>
        /// If you only want to download some of the files from git, put indexes of files in list. Starts at 0
        /// </summary>
        public List<int> GitFileIndexsToDownload { get; set; }

        /// <summary>
        /// The url to the githubApi releases page, which contains info about the releases
        /// </summary>
        public string GitApiReleasesURL { get; set; }

        /// <summary>
        /// The name of the project. Example: "BTDToolbox". Used for logging messages like: "Updating BTDToolbox"
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// The path to the project's exe. Example: "Environment.CurrentDirectory + "\\BTDToolbox.exe".
        /// Used to get the version number for the project
        /// </summary>
        public string ProjectExePath { get; set; }

        /// <summary>
        /// The EXE name of the updater. Example: "BTDToolbox Updater.exe". Used to launch the updater
        /// </summary>
        public string UpdaterExeName { get; set; }

        /// <summary>
        /// The name of the zip file that's downloaded from github releases if there is an update.
        /// Example: "BTDToolbox.zip". Used to delete the updater after updates
        /// </summary>
        public string UpdatedZipName { get; set; }

        /// <summary>
        /// The text read from GitApiReleasesURL
        /// </summary>
        private string AquiredGitApiText;

        public string InstallDirectory { get; set; } = Environment.CurrentDirectory;
        #endregion

        #region Constructors
        /// <summary>
        /// Check github releases for the latest release and install it if there is an update. 
        /// Need to manaully set properties with this constructor
        /// </summary>
        public Universal_Updater() { }

        /// <summary>
        /// Check github releases for the latest release and install it if there is an update.
        /// </summary>
        public Universal_Updater(string gitApiReleaseURL, string projectName, string projectExePath, string installDir, string updaterExeName, string updatedZipName)
        {
            GitApiReleasesURL = gitApiReleaseURL;
            ProjectName = projectName;
            ProjectExePath = projectExePath;
            InstallDirectory = installDir;
            UpdaterExeName = updaterExeName;
            UpdatedZipName = updatedZipName;
        }
        #endregion

        /// <summary>
        /// Main updater method. Handles all update related functions for ease of use.
        /// </summary>
        public void HandleUpdates(bool hasUpdater = true, bool closeProgram = true, bool deleteDownloadZip = false)
        {
            /*if (hasUpdater)   //commented our for now
                DeleteUpdater();*/    //delete updater if found to keep directory clean and prevent using old updater
            if (String.IsNullOrEmpty(GitApiReleasesURL))
            {
                Logger.Log("Can't get updates for " + ProjectName + " because the download URL has not been set.");
                return;
            }

            DeleteDownloadZip = deleteDownloadZip;

            GetGitApiText();
            if (String.IsNullOrEmpty(AquiredGitApiText))
            {
                Logger.Log("Failed to read release info for " + ProjectName);
                return;
            }

            if (!IsUpdate())
            {
                Logger.Log(ProjectName + " is up to date!");
                return;
            }

            var result = MessageBox.Show("An update is available for " + ProjectName + ". Would you like to download the update?", "Download update?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                Logger.Log("You chose not to install updates.");
                return;
            }

            Logger.Log("Downloading latest version...");
            DownloadUpdates();
            ExtractUpdater();


            if (closeProgram)
                Logger.Log("Closing " + ProjectName + "...", OutputType.Both);

            if (hasUpdater)
                LaunchUpdater();

            if (closeProgram)
                Environment.Exit(0);

            Logger.Log("Finished updating " + ProjectName + "!");
        }

        /// <summary>
        /// Get the text from the GitApiReleasesUrl
        /// </summary>
        /// <returns>The text on the page from the url</returns>
        private string GetGitApiText()
        {
            WebReader reader = new WebReader();
            AquiredGitApiText = reader.ReadText_FromURL(GitApiReleasesURL);
            return AquiredGitApiText;
        }

        /// <summary>
        /// Compare the latest release on github to see if an update is available for the main/executing program
        /// </summary>
        /// <returns>true or false, whether or not there is an update</returns>
        private bool IsUpdate() => IsUpdate(ProjectExePath, ProjectName, AquiredGitApiText);

        /// <summary>
        /// Compare the latest release on github to see if an update is 
        /// available for the program located at the path "exeToCheck"
        /// </summary>
        /// <param name="exeToCheck">The exe you want to check for an update</param>
        /// <param name="aquiredGitText">The url to the gitApi releases of the program you want to check for updates</param>
        /// <returns></returns>
        public static bool IsUpdate(string exeToCheck, string projectName, string aquiredGitText)
        {
            if (!File.Exists(exeToCheck))
            {
                Logger.Log(projectName + " files not found, need to redownload it.");
                return true;
            }

            string latestVersion_unparsed = GetLatestVersion(aquiredGitText).Replace(".", "");
            string latestVersion_parsed = "";
            foreach (var item in latestVersion_unparsed)
            {
                if (Int32.TryParse(item.ToString(), out var isValid))
                    latestVersion_parsed += item;
            }

            string currentVersion_unparsed = FileVersionInfo.GetVersionInfo(exeToCheck).FileVersion.Replace(".", "");
            string currentVersion_parsed = "";
            foreach (var item in currentVersion_unparsed)
            {
                if (Int32.TryParse(item.ToString(), out var isValid))
                    currentVersion_parsed += item;
            }

            while (currentVersion_parsed.Length != latestVersion_parsed.Length)
            {
                if (currentVersion_parsed.Length < latestVersion_parsed.Length)
                    currentVersion_parsed += "0";
                if (latestVersion_parsed.Length < currentVersion_parsed.Length)
                    latestVersion_parsed += "0";
            }

            Int32.TryParse(latestVersion_parsed, out int l);
            Int32.TryParse(currentVersion_parsed, out int c);
            return l > c;
        }

        /// <summary>
        /// Parses gitApi text and gets the latest release version of the main executing program
        /// </summary>
        /// <returns>an int of the latest release version, as a whole number without decimals</returns>
        private string GetLatestVersion() => GetLatestVersion(AquiredGitApiText);

        /// <summary>
        /// Parses gitApi text and gets the latest release version of the program the gitApi text is for
        /// </summary>
        /// <param name="aquiredGitText">The text that was successfully read from github</param>
        /// <returns>an int of the latest release version, as a whole number without decimals</returns>
        public static string GetLatestVersion(string aquiredGitText)
        {
            var gitApi = GithubReleaseConfig.FromJson(aquiredGitText);
            string latestRelease = gitApi[0].TagName;

            return latestRelease;
        }

        /// <summary>
        /// Reads gitApi text and gets all of the download urls associated with the latest release
        /// </summary>
        /// <returns>a list of download url strings</returns>
        private List<string> GetDownloadURLs()
        {
            int i = -1;
            List<string> downloads = new List<string>();
            var gitApi = GithubReleaseConfig.FromJson(AquiredGitApiText);
            foreach (var a in gitApi[0].Assets)
            {
                i++;
                if (GitFileIndexsToDownload != null && !GitFileIndexsToDownload.Contains(i))
                    continue;

                Logger.Log("Downloading " + a.BrowserDownloadUrl.ToString());
                downloads.Add(a.BrowserDownloadUrl.ToString());
            }

            return downloads;
        }

        /// <summary>
        /// Download all files aquired from the GetDownloadURLs method
        /// </summary>
        private void DownloadUpdates()
        {
            FileDownloader fileDownloader = new FileDownloader();

            List<string> downloads = GetDownloadURLs();
            foreach (string file in downloads)
                fileDownloader.DownloadFile(file, InstallDirectory);
        }

        /// <summary>
        /// Extract the updater from the downloaded zip
        /// </summary>
        private void ExtractUpdater()
        {
            var files = new DirectoryInfo(InstallDirectory).GetFiles();
            foreach (var file in files)
            {
                if (!file.Name.ToLower().Contains(UpdatedZipName.ToLower()))
                    continue;

                using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, entry.FullName));
                        if (destinationPath.StartsWith(Environment.CurrentDirectory, StringComparison.Ordinal))
                        {
                            if (new FileInfo(destinationPath).Name != "Updater.exe")
                                continue;

                            if (File.Exists(destinationPath))
                                File.Delete(destinationPath);

                            entry.ExtractToFile(destinationPath);
                            Logger.Log($"Extracting file: {destinationPath}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Launch the updater exe so the update can continue.
        /// </summary>
        private void LaunchUpdater()
        {
            string updater = Environment.CurrentDirectory + "\\" + UpdaterExeName;

            if (!File.Exists(updater))
            {
                Logger.Log("ERROR! Unable to find updater. You will need to close " + ProjectName +
                    " and manually extract " + UpdatedZipName);
                return;
            }

            Process.Start(updater, "launched_from_" + ProjectName);
            //Process.Start(updater);
        }

        /// <summary>
        /// Delete all files related to updater. Used to keep program directory clean
        /// </summary>
        public void DeleteUpdater()
        {
            if (File.Exists(InstallDirectory + "\\" + UpdaterExeName))
                File.Delete(InstallDirectory + "\\" + UpdaterExeName);

            var files = Directory.GetFiles(InstallDirectory);
            foreach (var file in files)
            {
                FileInfo f = new FileInfo(file);
                if (f.Name.ToLower().Replace(".", "").Replace(" ", "") == UpdatedZipName.ToLower().Replace(".", "").Replace(" ", ""))
                {
                    File.Delete(file);
                    break;
                }
            }
        }
    }
}
