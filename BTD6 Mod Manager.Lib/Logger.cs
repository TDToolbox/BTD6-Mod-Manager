using System;
using System.Diagnostics;

namespace BTD6_Mod_Manager.Lib
{
    /// <summary>
    /// Which medium do you want the message to output to?
    /// </summary>
    public enum OutputType
    {
        Console,
        MsgBox,
        Both
    }

    public class Logger
    {
        #region Properties

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        private static Logger instance;
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                    instance = new Logger();

                return instance;
            }
        }
        #endregion

        #region Events
        public static event EventHandler<LogEvents> MessageLogged;

        public class LogEvents : EventArgs
        {
            public string Message { get; set; }
            public OutputType Output { get; set; }
        }

        /// <summary>
        /// When a message has been sent to the Output() function
        /// </summary>
        /// <param name="e">LogEvent args containing the output message</param>
        public void OnMessageLogged(LogEvents e)
        {
            EventHandler<LogEvents> handler = MessageLogged;
            if (handler != null)
                handler(this, e);
        }

        #endregion


        /// <summary>
        /// Passes message to OnMessageLogged for Event Handling.
        /// </summary>
        /// <param name="text">Message to output to user</param>
        public static void Log(string text, OutputType output = OutputType.Console)
        {
            LogEvents args = new LogEvents();
            args.Output = output;

            string message = $">> {text}\n";
            args.Message = message;
            Debug.WriteLine(message);
            Instance.OnMessageLogged(args);
        }
    }
}