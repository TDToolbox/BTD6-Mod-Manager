using BTD6_Mod_Manager.Lib;
using BTD6_Mod_Manager.Lib.Game;
using BTD6_Mod_Manager.Lib.Natives;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace BTD6_Mod_Manager.Classes
{
    class Launcher
    {
        public static void Launch()
        {
            var gameInfo = GameInfo.GetGame(SessionData.CurrentGame);

            if (!Utility.IsProgramRunning(gameInfo.ProcName, out var btd6Proc))
                Process.Start("steam://rungameid/" + gameInfo.SteamID);
            else
            {
                Logger.Log("Please close BTD6 to continue...", OutputType.Both);
            }
        }
    }
}
