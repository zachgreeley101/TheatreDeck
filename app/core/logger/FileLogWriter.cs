using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace theatredeck.app.core.logger
{
    internal static class FileLogWriter
    {
        private static readonly object _lock = new object();
        private static string _logFilePath;

        public static void Initialize()
        {
            SetOrCreateLogFolderForToday();
            Logger.OnLogEntry += WriteLogEntryToFile;
        }

        private static void SetOrCreateLogFolderForToday()
        {
            // Build path: [ResourcePath]\logs\[MM-dd-yyyy]\
            string baseLogPath = Path.Combine(ConfigurationManager.AppSettings["ResourcePath"], "logs");
            if (!Directory.Exists(baseLogPath))
                Directory.CreateDirectory(baseLogPath);

            string todayFolderName = DateTime.Now.ToString("MM-dd-yyyy");
            string currentLogDirectory = Path.Combine(baseLogPath, todayFolderName);
            if (!Directory.Exists(currentLogDirectory))
                Directory.CreateDirectory(currentLogDirectory);

            _logFilePath = Path.Combine(currentLogDirectory, "logs.txt");
            if (!File.Exists(_logFilePath))
                File.WriteAllText(_logFilePath, string.Empty);
        }

        private static void WriteLogEntryToFile(LogEntry entry)
        {
            string logEntry = entry.ToString() + Environment.NewLine;
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logEntry, Encoding.UTF8);
            }
        }
    }
}
