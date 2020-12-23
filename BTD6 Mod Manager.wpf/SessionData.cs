using BTD6_Mod_Manager.IO;
using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using BTD6_Mod_Manager.Persistance;
using System;
using System.Collections.Generic;
using System.IO;

namespace BTD6_Mod_Manager
{
    public class SessionData
    {
        public static GameType currentGame = GameType.None;
        public static GameInfo GameInfo { get { return GameInfo.GetGame(currentGame); } }
        public static string ModsDir { get { return GetModsDir(); } }

        public static List<string> loadedMods = new List<string>();

        private static string GetModsDir()
        {
            if (currentGame != GameType.BTD6)
                return null;

            string modsDir = Settings.LoadedSettings.BTD6ModsDir;
            if (!String.IsNullOrEmpty(modsDir))
            {
                Directory.CreateDirectory(modsDir);
                return $"{modsDir}";
            }

            string gameDir = FileIO.AutoDetectGameDir();
            if (string.IsNullOrEmpty(gameDir))
            {
                Logger.Log("Failed to automatically find BTD6 directory. Please manually select it", OutputType.Both);
                gameDir = FileIO.BrowseForGameDir();
                if (String.IsNullOrEmpty(gameDir))
                {
                    Logger.Log("No game dir was selected. You'll need to chose a game dir to use the mod manager", OutputType.Both);
                    return null;
                }
            }

            modsDir = $"{gameDir}\\Mods";
            Directory.CreateDirectory(modsDir);
            Settings.LoadedSettings.BTD6ModsDir = modsDir;
            Settings.LoadedSettings.Save();
            return modsDir;
        }
    }
}