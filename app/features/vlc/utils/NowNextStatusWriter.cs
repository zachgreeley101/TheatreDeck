using System;
using System.IO;
using theatredeck.app.core.config; 
using theatredeck.app.core.utils;
using theatredeck.app.core.logger;
using theatredeck.app.features.vlc.models;

namespace theatredeck.app.features.vlc.utils
{
    /// <summary>
    /// Handles writing "Now Playing" and "Next Up" information to text files for overlays or external tools.
    /// Paths are configurable via App.config.
    /// </summary>
    public static class NowNextStatusWriter
    {
        private static readonly string NowPlayingFile;
        private static readonly string NextUpFile;

        static NowNextStatusWriter()
        {
            // Read the full file paths from App.config
            NowPlayingFile = ConfigManager.GetStringConfig("Vlc_NowPlayingFile", "NowPlaying.txt");
            NextUpFile = ConfigManager.GetStringConfig("Vlc_NextUpFile", "NextUp.txt");

            try
            {
                // Ensure directories exist for both files
                Directory.CreateDirectory(Path.GetDirectoryName(NowPlayingFile));
                Directory.CreateDirectory(Path.GetDirectoryName(NextUpFile));
                Logger.Info($"Verified status file directories: {NowPlayingFile} | {NextUpFile}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating file directories.", ex);
            }
        }


        /// <summary>
        /// Gets the preferred display text for a MediaItem: Notion name if available, else DisplayName.
        /// </summary>
        private static string GetDisplayText(MediaItem item, string fallback)
        {
            if (item == null)
                return fallback;

            // Always use the formatted DisplayName set by the MediaItemFactory
            return !string.IsNullOrWhiteSpace(item.DisplayName) ? item.DisplayName : fallback;
        }


        /// <summary>
        /// Writes the current "Now Playing" title to file.
        /// </summary>
        public static void WriteNowPlaying(MediaItem item)
        {
            string text = GetDisplayText(item, "No media playing");
            try
            {
                ClearFile(NowPlayingFile); // Clear before writing
                System.IO.File.WriteAllText(NowPlayingFile, text);
                Logger.Info($"Updated NowPlaying: '{text}'");
            }
            catch (Exception ex)
            {
                Logger.Error("Error writing NowPlaying.", ex);
            }
        }


        /// <summary>
        /// Writes the next media's title to file.
        /// </summary>
        public static void WriteNextUp(MediaItem item)
        {
            string text = GetDisplayText(item, "No next media");
            try
            {
                ClearFile(NextUpFile); // Clear the file first
                System.IO.File.WriteAllText(NextUpFile, text);
                Logger.Info($"Updated NextUp: '{text}'");
            }
            catch (Exception ex)
            {
                Logger.Error("Error writing NextUp.", ex);
            }
        }



        /// <summary>
        /// Optionally, write both Now Playing and Next Up in one call.
        /// </summary>
        public static void WriteNowAndNext(MediaItem now, MediaItem next)
        {
            Logger.Debug(
                $"WriteNowAndNext called. NowPlaying='{GetDisplayText(now, "null")}', NextUp='{GetDisplayText(next, "null")}'");

            WriteNowPlaying(now);
            WriteNextUp(next);

            Logger.Debug(
                $"WriteNowAndNext complete. NowPlayingFile='{NowPlayingFile}', NextUpFile='{NextUpFile}'");
        }


        /// <summary>
        /// Clears the now/next files (e.g., when playback stops).
        /// </summary>
        public static void ClearStatus()
        {
            try
            {
                File.WriteAllText(NowPlayingFile, string.Empty);
                File.WriteAllText(NextUpFile, string.Empty);
                Logger.Info("Cleared NowPlaying and NextUp files.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error clearing status files.", ex);
            }
        }


        public static void WritePlaybackTimes(int currentSeconds, int totalSeconds)
        {
            string nowPlayingTimeFile = ConfigManager.GetStringConfig("Vlc_TimeNowPlayingFile", "TimeNowPlaying.txt");
            string nextUpTimeFile = ConfigManager.GetStringConfig("Vlc_TimeNextUpFile", "TimeNextUp.txt");

            int remaining = Math.Max(0, totalSeconds - currentSeconds);

            try
            {
                ClearFile(nowPlayingTimeFile); // Clear before writing
                ClearFile(nextUpTimeFile);     // Clear before writing

                System.IO.File.WriteAllText(nowPlayingTimeFile, currentSeconds.ToString());
                System.IO.File.WriteAllText(nextUpTimeFile, remaining.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error("Error writing playback times.", ex);
            }
        }



        public static void WritePauseToTimeFiles()
        {
            string nowPlayingTimeFile = ConfigManager.GetStringConfig("Vlc_TimeNowPlayingFile", "TimeNowPlaying.txt");
            string nextUpTimeFile = ConfigManager.GetStringConfig("Vlc_TimeNextUpFile", "TimeNextUp.txt");
            try
            {
                ClearFile(nowPlayingTimeFile); // Clear before writing
                ClearFile(nextUpTimeFile);     // Clear before writing

                System.IO.File.WriteAllText(nowPlayingTimeFile, "pause");
                System.IO.File.WriteAllText(nextUpTimeFile, "pause");
                Logger.Info("Wrote 'pause' to time files.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error writing 'pause' to time files.", ex);
            }
        }


        public static void WriteStopToTimeFiles()
        {
            string nowPlayingTimeFile = ConfigManager.GetStringConfig("Vlc_TimeNowPlayingFile", "TimeNowPlaying.txt");
            string nextUpTimeFile = ConfigManager.GetStringConfig("Vlc_TimeNextUpFile", "TimeNextUp.txt");
            try
            {
                ClearFile(nowPlayingTimeFile); // Clear before writing
                ClearFile(nextUpTimeFile);     // Clear before writing

                System.IO.File.WriteAllText(nowPlayingTimeFile, "stop");
                System.IO.File.WriteAllText(nextUpTimeFile, "stop");
                Logger.Info("Wrote 'stop' to time files.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error writing 'stop' to time files.", ex);
            }
        }


        private static void ClearFile(string filePath)
        {
            try
            {
                System.IO.File.WriteAllText(filePath, string.Empty); // Empties the file
            }
            catch (Exception ex)
            {
                Logger.Error($"Error clearing file '{filePath}'.", ex);
            }
        }

    }
}
