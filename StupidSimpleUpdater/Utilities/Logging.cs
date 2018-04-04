using System;
using System.IO;

namespace StupidSimpleUpdater.Utilities
{
    internal static class Logging
    {
        #region fields

        private static string logLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\StupidSimpleUpdater\log.txt";

        #endregion

        #region methods

        public static void ClearLog()
        {
            File.WriteAllText(logLocation, String.Empty);
        }

        public static void Info(string message)
        {
            WriteLogWithHeader("[INFO]", message);
        }

        public static void Warning(string message)
        {
            WriteLogWithHeader("[WARNING]", message);
        }

        public static void Error(string message)
        {
            WriteLogWithHeader("[ERROR]", message);
        }

        private static void WriteLogWithHeader(string header, string message)
        {
            using (StreamWriter streamWriter = File.AppendText(logLocation))
            {
                streamWriter.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), header, message));
            }
        }

        #endregion
    }
}
