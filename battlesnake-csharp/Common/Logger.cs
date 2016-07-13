using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace battlesnake_csharp.Common
{
    public class Logger
    {
        private static string logFile = System.AppDomain.CurrentDomain.BaseDirectory + "log.txt";

        public static void Log(string tag, string message)
        {
            string logMessage = string.Format("{0}: {1}: {2}", DateTime.Now, tag, message);
            File.AppendAllText(logFile, logMessage + Environment.NewLine);
        }
    }
}