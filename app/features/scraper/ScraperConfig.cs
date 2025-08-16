using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using theatredeck.app.core.logger;

namespace theatredeck.app.features.scraper
{
    /// <summary>
    /// Loads and provides scraper-specific configuration settings from app.config.
    /// </summary>
    internal class ScraperConfig
    {
        /// <summary>
        /// The root server path containing all media drives.
        /// </summary>
        public string ServerPath { get; private set; }

        /// <summary>
        /// Dictionary mapping drive labels (e.g., "Media Drive 2") to their movie folder paths.
        /// </summary>
        public Dictionary<string, string> MediaDrivePaths { get; private set; }

        /// <summary>
        /// Loads configuration settings on construction.
        /// </summary>
        public ScraperConfig()
        {
            try
            {
                ServerPath = ConfigurationManager.AppSettings["ServerPath"] ?? "";
                if (string.IsNullOrWhiteSpace(ServerPath))
                {
                    Logger.Error("[CONFIG ERROR] ServerPath missing or empty in app.config.");
                }
                else
                {
                    Logger.Info($"[CONFIG] Loaded ServerPath: {ServerPath}");
                }

                MediaDrivePaths = new Dictionary<string, string>();

                for (int i = 1; i <= 6; i++)
                {
                    string driveKey = $"MediaDrive{i}";
                    string defaultPath = Path.Combine(ServerPath, $"Media Drive {i}", $"Movies_{i}");
                    string drivePath = ConfigurationManager.AppSettings[driveKey] ?? defaultPath;

                    MediaDrivePaths.Add($"Media Drive {i}", drivePath);

                    if (!Directory.Exists(drivePath))
                    {
                        Logger.Warning($"[CONFIG WARNING] Directory for {driveKey} does not exist: {drivePath}");
                    }
                    else
                    {
                        Logger.Info($"[CONFIG] Loaded path for {driveKey}: {drivePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[CONFIG EXCEPTION] Error initializing ScraperConfig: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the movie folder path for a given drive name (e.g., "Media Drive 2").
        /// Returns null if not found.
        /// </summary>
        public string GetMediaDrivePath(string driveName)
        {
            if (MediaDrivePaths.TryGetValue(driveName, out var path))
                return path;

            Logger.Error($"[CONFIG ERROR] Requested drive name '{driveName}' not found in MediaDrivePaths.");
            return null;
        }
    }
}
