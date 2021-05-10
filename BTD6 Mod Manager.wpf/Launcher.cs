using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using BTD6_Mod_Manager.Lib.MelonMods;
using BTD6_Mod_Manager.Lib.Natives;
using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static BTD6_Mod_Manager.Lib.MelonMods.MelonMod_Handler;

namespace BTD6_Mod_Manager
{
    class Launcher
    {
        public static void Launch()
        {
            if (!AreModsValid())
                return;

            var gameInfo = GameInfo.GetGame(SessionData.currentGame);

            if (!Utility.IsProgramRunning(gameInfo.ProcName, out var btd6Proc))
                Process.Start("steam://rungameid/" + gameInfo.SteamID);
            else
                Logger.Log("Please close BTD6 to continue...", OutputType.Both);
        }

        private static bool AreModsValid()
        {
            foreach (var mod in SessionData.loadedMods)
            {
                string filePath = mod;
                if (IsFileZip(filePath))
                    continue;

                var melonInfo = MelonMod_Handler.GetModInfo(filePath);
                string melonModName = string.IsNullOrEmpty(melonInfo?.Name) ? "" : melonInfo.Name;

                var similarMods = SessionData.loadedMods.Count(dupMod => GetModInfo(dupMod)?.Name == melonModName);
                bool isDuplicate = (similarMods > 1);
                if (!isDuplicate)
                    continue;

                Logger.Log($"Error! You are trying to load \"{melonModName}\" more than once. " +
                    $"You can only have one of \"{melonModName}\" active at a time.", OutputType.Both);
                return false;
            }

            return true;
        }

        private static bool IsFileZip(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            bool isZip = (fileInfo.Extension == ".zip" || fileInfo.Extension == ".rar" || fileInfo.Extension == ".7z");
            return isZip;
        }
    }
}
