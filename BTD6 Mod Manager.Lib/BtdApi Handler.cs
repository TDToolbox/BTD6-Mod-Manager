using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BTD6_Mod_Manager.Lib
{
    public class BtdApi_Handler
    {
        public static bool DoesInjectorExist(string modsDir)
        {
            const string disabledKey = ".disabled";
            bool injectorExists = File.Exists($"{modsDir}\\BtdAPI_Injector.dll");
            bool disabledInjectorExists = File.Exists($"{modsDir}\\BtdAPI_Injector.dll{disabledKey}");

            return (injectorExists || disabledInjectorExists);
        }

        public static void AskDownloadInjector()
        {
            var result = MessageBox.Show("The Injector for BTD6 API mods was not found. It is required in order to use BTD6 API mods." +
                        " Do you want to download it?", "Download BTD6 API Injector?", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Process.Start("https://www.nexusmods.com/bloonstd6/mods/41?tab=description");
            }
            else
            {
                Logger.Log("You chose not to download the injector. You're BTD6 API mods won't work until you get it" +
                    ". However, all other BTD6 mods will continue to work.", OutputType.Both);
            }
        }
    }
}
