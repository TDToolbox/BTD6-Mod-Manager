using BTD6_Mod_Manager.Lib;
using System;
using System.Collections.Generic;
using System.IO;

namespace BTD6_Mod_Manager.Persistance
{
    internal class Settings
    {
        public static string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TD Loader";
        public static string settingsFilePath = $"{settingsDir}\\settings.json";
        private static Settings settings;

        #region Properties
        public bool IsNewUser { get; set; } = true;
        public bool ConsoleFlash { get; set; } = true;
        public bool LoadedFirstMod { get; set; }
        public bool ShownBtdApiInjectorMessage { get; set; }
        public string BTD6ModsDir { get; set; }
        public List<string> LastUsedMods { get; set; } = new List<string>();
        public static Settings LoadedSettings
        {
            get 
            {
                if (settings is null)
                    settings = Load();

                return settings;
            }
            set { settings = value; }
        }
        #endregion


        private static Settings Load()
        {
            return File.Exists(settingsFilePath) ? Serializer.LoadFromFile<Settings>(settingsFilePath) : new Settings();
        }

        public void Save()
        {
            Serializer.SaveToFile<Settings>(LoadedSettings, settingsFilePath);
        }
    }
}
