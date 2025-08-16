using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace theatredeck.app.core.logger
{
    internal static class Logger
    {
        // Minimum log level to record. Default: Debug (change as needed)
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        // Event for subscribers (such as FileLogWriter)
        public static event Action<LogEntry> OnLogEntry;

        // Central logging method with automatic caller info
        public static void Log(
            LogLevel level,
            string message,
            Exception ex = null,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (level < MinimumLevel)
                return;

            string source = $"{Path.GetFileNameWithoutExtension(file)}::{member}({line})";
            var entry = new LogEntry(level, message, source, ex);

            // Always write to Debug output for live debugging
            System.Diagnostics.Debug.WriteLine(entry.ToString());

            // Raise event for log file, UI, etc.
            OnLogEntry?.Invoke(entry);
        }

        // Shortcut methods for each log level
        public static void Debug(string message, Exception ex = null,
            [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            Log(LogLevel.Debug, message, ex, member, file, line);
        }

        public static void Info(string message, Exception ex = null,
            [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            Log(LogLevel.Info, message, ex, member, file, line);
        }

        public static void Warning(string message, Exception ex = null,
            [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            Log(LogLevel.Warning, message, ex, member, file, line);
        }

        public static void Error(string message, Exception ex = null,
            [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            Log(LogLevel.Error, message, ex, member, file, line);
        }

        public static void Critical(string message, Exception ex = null,
            [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            Log(LogLevel.Critical, message, ex, member, file, line);
        }
    }
}