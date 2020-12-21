using BTD6_Mod_Manager.Lib.Game;
using System.Collections.Generic;

namespace BTD6_Mod_Manager.Classes
{
    class SessionData
    {
        public static GameType CurrentGame = GameType.None;
        public static string CurrentGameModDir = "";
        public static List<string> LoadedMods = new List<string>();
    }
}
