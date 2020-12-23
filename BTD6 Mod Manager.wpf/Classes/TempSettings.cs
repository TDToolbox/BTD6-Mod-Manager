using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BTD6_Mod_Manager.Classes
{
    /*class TempSettings
    {
        private static TempSettings instance;

        public static TempSettings Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new TempSettings();
                    instance = instance.LoadSettings();
                }

                return instance; 
            }
            set { instance = value; }
        }

        public string settingsFileName { get; set; } = "settings.json";
        public string MainSettingsDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TD Loader";
        public bool IsNewUser { get; set; } = true;
        public bool ConsoleFlash { get; set; } = true;
        public bool LoadedFirstMod { get; set; } = false;
        public bool ShownBtdApiInjectorMessage { get; set; } = false;
        public GameType LastGame { get; set; } = GameType.BTD6;
        public List<string> LastUsedMods { get; set; } = new List<string>();
        
        [JsonProperty]
        private string BTD6_ModsDir { get; set; }
        
        public TempSettings()
        {
            *//*settingsFileName = "settings.json";*/
            /*MainSettingsDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TD Loader";*//*
        }

        public TempSettings LoadSettings()
        {
            if (!File.Exists(Instance.MainSettingsDir + "\\" + Instance.settingsFileName))
            {
                CreateNewSettings();
                return this;
            }

            string json = File.ReadAllText(MainSettingsDir + "\\" + settingsFileName);
            if (!Guard.IsJsonValid(json) || json.Length <= 0)
            {
                Logger.Log("Settings file has invalid json, generating a new settings file.");
                CreateNewSettings();
                return this;
            }

            var abc = JsonConvert.DeserializeObject<TempSettings>(json);
            SessionData.LoadedMods = abc.LastUsedMods;
            return abc;
        }

        public void SaveSettings()
        {
            LastUsedMods = SessionData.LoadedMods;

            string output = JsonConvert.SerializeObject(this, Formatting.Indented);
            if (!Directory.Exists(MainSettingsDir))
                Directory.CreateDirectory(MainSettingsDir);

            string path = MainSettingsDir + "\\" + settingsFileName;
            try
            {
                StreamWriter serialize = new StreamWriter(path, false);
                serialize.Write(output);
                serialize.Close();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, OutputType.Both);
            }
        }

        public void CreateNewSettings()
        {
            BTD6_ModsDir = SteamUtils.GetGameDir(GameType.BTD6) + "\\Mods";
            Logger.Log(BTD6_ModsDir, OutputType.Both);
            SaveSettings();
        }

        public string GetModsDir() => GetModsDir(SessionData.CurrentGame);

        public string GetModsDir(GameType game)
        {
            string modsDir = BTD6_ModsDir;
            if (String.IsNullOrEmpty(modsDir))
                Logger.Log("Mods Directory is null. Please create a Mods folder in your BTD6 folder", OutputType.Both);

            Directory.CreateDirectory(modsDir);
            return modsDir;
        }

        public void SetModsDir(string path) => SetModsDir(SessionData.CurrentGame, path);
        public void SetModsDir(GameType game, string path)
        {
            BTD6_ModsDir = path;
            SaveSettings();
        }
    }*/
}