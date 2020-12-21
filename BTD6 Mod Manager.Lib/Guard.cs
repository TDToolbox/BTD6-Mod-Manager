using System;
using System.IO;

namespace BTD6_Mod_Manager.Lib
{
    /// <summary>
    /// This class contians common checks used throught the code
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Check if an object is null and throw an Argument exception if it is
        /// </summary>
        /// <param name="obj">Object to check if null</param>
        public static void ThrowIfArgumentIsNull(object obj, string argumentName, string message = "")
        {
            if (obj != null)
                return;

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(argumentName);
            else
                throw new ArgumentNullException(argumentName, message);
        }


        public static void ThrowIfStringIsNull(string stringToCheck, string message)
        {
            if (String.IsNullOrEmpty(stringToCheck))
            {
                throw new Exception(message);
            }
        }


        /// <summary>
        /// Check if FileInfo file contains valid json
        /// </summary>
        /// <param name="file">FileInfo to check</param>
        /// <returns>Whether or not FileInfo file contains valid json</returns>
        public static bool IsJsonValid(FileInfo file) => IsJsonValid(File.ReadAllText(file.FullName));

        /// <summary>
        /// Check if text is valid json
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>Whether or not text is valid json</returns>
        public static bool IsJsonValid(string text)
        {
            try
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                dynamic result = serializer.DeserializeObject(text);
                return true;
            }
            catch { return false; }
        }
    }
}
