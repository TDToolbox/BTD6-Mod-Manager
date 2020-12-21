using BTD6_Mod_Manager.Lib;

namespace BTD6_Mod_Manager.Classes
{
    class TempGuard
    {
        public static bool IsDoingWork(string errorMessage)
        {
            if (MainWindow.doingWork)
            {
                Logger.Log("Cant do that! Doing something else.\nCurrent Process: " + errorMessage, OutputType.Both);
                return true;
            }
            else
                return false;
        }
    }
}
