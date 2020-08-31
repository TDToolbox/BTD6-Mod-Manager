using BTD_Backend.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6_Mod_Manager.Classes
{
    class SessionData
    {
        public static GameType CurrentGame = GameType.None;
        public static string CurrentGameModDir = "";
        public static List<string> LoadedMods = new List<string>();
    }
}
