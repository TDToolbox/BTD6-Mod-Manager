using BTD6_Mod_Manager.Lib.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6_Mod_Manager.IO
{
    public class FileIO : Lib.IO.FileIO
    {
        public static string BrowseForGameDir(string windowTitle = "Browse for BloonsTD6 directory")
        {
            return BrowseForDirectory(windowTitle, Environment.CurrentDirectory);
        }

        public static string AutoDetectGameDir() => AutoDetectGameDir(SessionData.currentGame);
        public static string AutoDetectGameDir(GameType gameType)
        {
            return SteamUtils.GetGameDir(gameType);
        }
    }
}
