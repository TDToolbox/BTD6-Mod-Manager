using System;

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

    public class Log
    {
        #region Properties

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        private static Log instance;
        public static Log Instance
        {
            get
            {
                if (instance == null)
                    instance = new Log();

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
        public static void Output(string text, OutputType output = OutputType.Console)
        {
            LogEvents args = new LogEvents();
            args.Output = output;
            args.Message = ">> " + text + Environment.NewLine;
            Instance.OnMessageLogged(args);
        }
    }
}