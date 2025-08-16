using System;

namespace theatredeck.app.features.vlc.utils
{
    /// <summary>
    /// Central source for media start and end timing.
    /// In the future, can be extended to fetch from a database.
    /// </summary>
    public static class MediaTimingProvider
    {
        // Hardcoded defaults (can be modified for testing)
        public const int DefaultStartTimeSeconds = 0;
        public const int DefaultEndTimeSeconds = 1800000;

        /// <summary>
        /// Returns start and end timing for the given media file.
        /// In the future, this can be a DB lookup by filePath.
        /// </summary>
        public static (int StartTimeSeconds, int EndTimeSeconds) GetTimingFor(string filePath)
        {
            // For now, always return defaults.
            // Future: Query database for specific timing by filePath.
            return (DefaultStartTimeSeconds, DefaultEndTimeSeconds);
        }
    }
}
