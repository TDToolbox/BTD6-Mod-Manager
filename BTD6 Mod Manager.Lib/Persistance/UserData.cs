using BTD6_Mod_Manager.Lib.Game;
using BTD6_Mod_Manager.Lib.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BTD6_Mod_Manager.Lib.Persistance
{
    /// <summary>
    /// Manages persistence data related to user, such as game locations, if they are a new user, etc.
    /// This data is not intended to be configurable, therefore this is not the same as config/settings
    /// </summary>
    public class UserData
    {
        public static string MainProgramName;
        public static string MainProgramExePath;
        public static string MainSettingsDir;
        public static string UserDataFilePath;


        private static UserData _instance;

        public static UserData Instance
        {
            get 
            {
                if (_instance == null)
                    _instance = LoadUserData();
                return _instance; 
            }
            set { _instance = value; }
        }


        /// <summary>
        /// Manages the last known version of the executing program
        /// </summary>
        public string MainProgramVersion { get; set; } = "";

        /// <summary>
        /// Has the user just updated the executing program?
        /// </summary>
        public bool RecentUpdate { get; set; } = true;

        /// <summary>
        /// Is this this a new user?
        /// </summary>
        public bool NewUser { get; set; } = true;


        /// <summary>
        /// BTD5 Data
        /// </summary>
        #region BTD5
        private static GameInfo btd5 = GameInfo.GetGame(GameType.BTD5);
        public string BTD5Dir { get; set; } = btd5.GameDir;
        public string BTD5Version { get; set; } = FileIO.GetFileVersion(btd5.GameDir + "\\" + btd5.EXEName);
        public string BTD5BackupDir { get; set; } = Environment.CurrentDirectory + "\\Backups\\" + btd5.Type.ToString();
        #endregion

        /// <summary>
        /// BTDB Data
        /// </summary>
        #region BTDB
        private static GameInfo btdb = GameInfo.GetGame(GameType.BTDB);
        public string BTDBDir { get; set; } = btdb.GameDir;
        public string BTDBVersion { get; set; } = FileIO.GetFileVersion(btdb.GameDir + "\\" + btdb.EXEName);
        public string BTDBBackupDir { get; set; } = Environment.CurrentDirectory + "\\Backups\\" + btdb.Type.ToString();
        #endregion

        /// <summary>
        /// Bloons Monkey City Data
        /// </summary>
        #region Monkey City
        private static GameInfo bmc = GameInfo.GetGame(GameType.BMC);
        public string BMCDir { get; set; } = bmc.GameDir;
        public string BMCVersion { get; set; } = FileIO.GetFileVersion(bmc.GameDir + "\\" + bmc.EXEName);
        public string BMCBackupDir { get; set; } = Environment.CurrentDirectory + "\\Backups\\" + bmc.Type.ToString();
        #endregion

        /// <summary>
        /// BTD6 Data
        /// </summary>
        #region BTD6
        private static GameInfo btd6 = GameInfo.GetGame(GameType.BTD6);
        public string BTD6Dir { get; set; }// = btd6.GameDir;
        public string BTD6Version { get; set; } = FileIO.GetFileVersion(btd6.GameDir + "\\" + btd6.EXEName);
        public string BTD6BackupDir { get; set; } = Environment.CurrentDirectory + "\\Backups\\" + btd6.Type.ToString();
        #endregion

        /// <summary>
        /// Bloons Adventure Time Data
        /// </summary>
        #region BTDAT
        private static GameInfo btdat = GameInfo.GetGame(GameType.BTDAT);
        public string BTDATDir { get; set; } = btdat.GameDir;
        public string BTDATVersion { get; set; } = FileIO.GetFileVersion(btdat.GameDir + "\\" + btdat.EXEName);
        public string BTDATBackupDir { get; set; } = Environment.CurrentDirectory + "\\Backups\\" + btdat.Type.ToString();
        #endregion

        /// <summary>
        /// NKArchive Data
        /// </summary>
        #region BTDAT
        private static GameInfo nkArchive = GameInfo.GetGame(GameType.NKArchive);
        public string NKArchiveDir { get; set; } = nkArchive.GameDir;
        public string NKArchiveVersion { get; set; } = FileIO.GetFileVersion(nkArchive.GameDir + "\\" + nkArchive.EXEName);
        public string NKArchiveBackupDir { get; set; } = Environment.CurrentDirectory + "\\Backups\\" + nkArchive.Type.ToString();


        public List<string> PreviousProjects { get; set; }
        #endregion


        #region Constructors
        public UserData()
        {
            /*if (!Guard.IsStringValid(MainProgramName))
                throw new MainProgramNameNotSet();

            if (!Guard.IsStringValid(MainProgramExePath))
                throw new MainProgramExePathNotSet();*/

            if (String.IsNullOrEmpty(MainSettingsDir))
                MainSettingsDir = Environment.CurrentDirectory;

            if (String.IsNullOrEmpty(MainProgramVersion))
                MainProgramVersion = FileVersionInfo.GetVersionInfo(MainProgramExePath).FileVersion;

            if (!Directory.Exists(MainSettingsDir))
                Directory.CreateDirectory(MainSettingsDir);

            if (String.IsNullOrEmpty(UserDataFilePath))
                UserDataFilePath = MainSettingsDir + "\\userdata.json";

            if (PreviousProjects == null)
                PreviousProjects = new List<string>();

            UserDataLoaded += UserData_UserDataLoaded;
        }

        private void UserData_UserDataLoaded(object sender, UserDataEventArgs e)
        {
            if (string.IsNullOrEmpty(btd6.GameDir) && !string.IsNullOrEmpty(BTD6Dir))
                btd6.GameDir = BTD6Dir;
        }

        #endregion

        /// <summary>
        /// Open the main settings directory
        /// </summary>
        public static void OpenSettingsDir()
        {
            if (Instance == null)
                Instance = new UserData();

            if (!Directory.Exists(MainSettingsDir))
                Directory.CreateDirectory(MainSettingsDir);

            Process.Start(MainSettingsDir);
        }

        /// <summary>
        /// Load userdata from file
        /// </summary>
        /// <returns>The loaded userdata</returns>
        public static UserData LoadUserData()
        {
            /*if (Instance == null)
                Instance = new UserData();*/

            var user = new UserData();

            if (!File.Exists(UserDataFilePath))
            {
                user.OnUserDataLoaded(new UserDataEventArgs());
                return user;
            }

            string json = File.ReadAllText(UserDataFilePath);
            if (json == "null" || string.IsNullOrEmpty(json) || !Guard.IsJsonValid(json))
            {
                Logger.Log("Userdata has invalid json, generating a new one.");
                user = new UserData();
                user.OnUserDataLoaded(new UserDataEventArgs());
                return user;
            }

            user = JsonConvert.DeserializeObject<UserData>(json);
            user.OnUserDataLoaded(new UserDataEventArgs());
            return user;
        }

        /// <summary>
        /// Save userdata to file
        /// </summary>
        public static void SaveUserData()
        {
            if (Instance == null)
                LoadUserData();

            string output = JsonConvert.SerializeObject(Instance, Formatting.Indented);

            StreamWriter serialize = new StreamWriter(UserDataFilePath, false);
            serialize.Write(output);
            serialize.Close();
        }

        #region Events
        public static event EventHandler<UserDataEventArgs> UserDataLoaded;


        /// <summary>
        /// Events related to JetPasswords
        /// </summary>
        public class UserDataEventArgs : EventArgs
        {

        }

        /// <summary>
        /// Fired when the password list was successfully aquired. Passes password list as arg
        /// </summary>
        /// <param name="e">JetPasswordEvetnArgs takes the aquired password list as an argument</param>
        public void OnUserDataLoaded(UserDataEventArgs e)
        {
            EventHandler<UserDataEventArgs> handler = UserDataLoaded;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}
