using BTD6_Mod_Manager.Lib.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace BTD6_Mod_Manager.Lib.Updaters
{
    class MelonLoader_Updater
    {
        internal static readonly string defaultGitURL = "https://api.github.com/repos/HerpDerpinstine/MelonLoader/releases";

        public string GitReleasesURL { get; set; } = "";
        public string MelonHandlerDllPath { get; set; }
        public string DownloadDir { get; set; }
        public GithubReleaseConfig LatestReleaseInfo { get; set; }


        public MelonLoader_Updater(string downloadDir, string melonHandlerDllPath)
        {
            GitReleasesURL = defaultGitURL;
            DownloadDir = downloadDir;
            MelonHandlerDllPath = melonHandlerDllPath;
        }

        public MelonLoader_Updater(string gitReleaseURL, string downloadDir, string melonHandlerDllPath)
        {
            GitReleasesURL = (String.IsNullOrEmpty(gitReleaseURL)) ? defaultGitURL : gitReleaseURL;
            DownloadDir = downloadDir;
            MelonHandlerDllPath = melonHandlerDllPath;
        }


        public void HandleUpdates()
        {
            var unparsedGitText = GetGitText();
            if (string.IsNullOrEmpty(unparsedGitText))
                return;

            var releaseConfig = CreateReleaseConfigFromText(unparsedGitText);
            if (releaseConfig is null)
                return;

            bool isUpdate = IsUpdate(releaseConfig);
            if (!File.Exists(MelonHandlerDllPath))
                isUpdate = true;

            if (!isUpdate)
            {
                Logger.Log("MelonLoader is up to date!");
                return;
            }

            if (isUpdate && File.Exists(MelonHandlerDllPath))
            {
                Logger.Log("An update is available for MelonLoader!");
                bool installUpdates = AskInstallUpdates();
                if (!installUpdates)
                {
                    Logger.Log("You chose not to install updates.");
                    return;
                }
            }
            else if (isUpdate && !File.Exists(MelonHandlerDllPath))
                Logger.Log("MelonLoader is not installed. Installing MelonLoader");

            DownloadUpdates();
            ExtractUpdater();
            
            if (File.Exists(MelonHandlerDllPath))
                Logger.Log("Successfully installed MelonLoader", OutputType.Both);
            else
                Logger.Log("Failed to install MelonLoader. You will need to install it yourself", OutputType.Both);
        }


        private string GetGitText()
        {
            WebReader reader = new WebReader();
            var gitText = reader.ReadText_FromURL(GitReleasesURL, maxTries: 50);
            return gitText;
        }

        private List<GithubReleaseConfig> CreateReleaseConfigFromText(string unparsedGitText)
        {
            var releaseConfig = GithubReleaseConfig.FromJson(unparsedGitText);
            return releaseConfig;
        }

        private bool IsUpdate(List<GithubReleaseConfig> githubReleaseConfigs)
        {
            LatestReleaseInfo = githubReleaseConfigs[0];
            if (!File.Exists(MelonHandlerDllPath))
                return true;

            GetCurrentAndLatestVersion(out string latestGitVersion, out string currentVersion);
            var latest = VersionToInt(latestGitVersion);
            var current = VersionToInt(currentVersion);
            return latest > current;
        }

        private void GetCurrentAndLatestVersion(out string latestGitVersion, out string currentVersion)
        {
            latestGitVersion = LatestReleaseInfo.TagName;
            currentVersion = GetCurrentVersion();
            CleanVersionTexts(latestGitVersion, currentVersion, out latestGitVersion, out currentVersion);
        }

        private string GetCurrentVersion()
        {
            FileVersionInfo currentVersionInfo = FileVersionInfo.GetVersionInfo(MelonHandlerDllPath);
            return currentVersionInfo.FileVersion;
        }

        private void CleanVersionTexts(string latestGitVersion, string currentVersion, out string processedLatestVersion, out string processedCurrentVersion)
        {
            const string delimiter = ".";
            processedLatestVersion = latestGitVersion.Replace(delimiter, "").Replace("v", "");
            processedCurrentVersion = currentVersion.Replace(delimiter, "");
            bool areSameLength = (processedLatestVersion.Length == processedCurrentVersion.Length);
            if (areSameLength)
                return;

            const string fillerChar = "0";
            while (!areSameLength)
            {
                int lLength = processedLatestVersion.Length;
                int cLength = currentVersion.Length;

                processedLatestVersion = (lLength < cLength) ? processedLatestVersion + fillerChar : processedLatestVersion;
                processedCurrentVersion = (cLength < lLength) ? processedCurrentVersion + fillerChar : processedCurrentVersion;
                areSameLength = (processedLatestVersion.Length == processedCurrentVersion.Length);
            }
        }

        private int VersionToInt(string versionText)
        {
            Int32.TryParse(versionText, out int version);
            return version;
        }


        private bool AskInstallUpdates()
        {
            var result = MessageBox.Show("An update is available for MelonLoader. Would you like to download the update?", "Download update?", MessageBoxButton.YesNo);
            return (result == MessageBoxResult.Yes);
        }

        private void DownloadUpdates()
        {
            List<string> downloads = GetDownloadURLs();
            foreach (string file in downloads)
            {
                FileDownloader downloader = new FileDownloader();
                Logger.Log(file);
                Logger.Log(DownloadDir);
                downloader.DownloadFile(file, DownloadDir);
            }
        }

        private List<string> GetDownloadURLs()
        {
            const string requiredFileEnding = "x64.zip";
            List<string> downloads = new List<string>();
            foreach (var asset in LatestReleaseInfo.Assets)
            {
                if (!asset.Name.EndsWith(requiredFileEnding))
                    continue;

                Logger.Log("Downloading " + asset.BrowserDownloadUrl.ToString());
                downloads.Add(asset.BrowserDownloadUrl.ToString());
            }

            return downloads;
        }

        private void ExtractUpdater()
        {
            var files = new DirectoryInfo(DownloadDir).GetFiles("*.zip");
            foreach (var file in files)
            {
                string filename = file.Name;
                if (!filename.ToLower().Contains("melonloader"))
                    continue;

                using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(DownloadDir, entry.FullName));
                        if (destinationPath.StartsWith(DownloadDir, StringComparison.Ordinal))
                        {
                            if (String.IsNullOrEmpty(entry.Name))
                                continue;

                            var fileInfo = new FileInfo(destinationPath);
                            fileInfo.Directory.Create();
                            entry.ExtractToFile(destinationPath, true);
                        }
                    }
                }
            }
        }
    }
}