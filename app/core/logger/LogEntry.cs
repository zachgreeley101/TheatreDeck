using System;

namespace theatredeck.app.core.logger
{
    internal class LogEntry
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public LogLevel Level { get; }
        public string Message { get; }
        public string Source { get; }
        public Exception Exception { get; }

        public LogEntry(LogLevel level, string message, string source, Exception exception = null)
        {
            Level = level;
            Message = message;
            Source = source;
            Exception = exception;
        }

        public override string ToString()
        {
            var exMsg = Exception != null ? $" Exception: {Exception}" : "";
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{Source}] {Message}{exMsg}";
        }
    }
}
