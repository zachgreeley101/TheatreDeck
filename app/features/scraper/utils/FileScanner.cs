using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using theatredeck.app.core.logger;

namespace theatredeck.app.features.scraper.utils
{
    /// <summary>
    /// Utility for enumerating movie folders and identifying valid media files within each folder.
    /// </summary>
    internal class FileScanner
    {
        private static readonly string[] ValidExtensions = { ".mkv", ".mp4", ".avi", ".mov" };

        /// <summary>
        /// Scans the given drive's movie root directory for subfolders containing exactly one media file.
        /// Returns a list of tuples: (folder path, media file path).
        /// </summary>
        /// <param name="moviesRootPath">Root path of the media drive's movies (e.g., "Media Drive 2/Movies_2/")</param>
        /// <returns>List of (movie folder, media file) pairs that are valid for scraping.</returns>
        public static List<(string FolderPath, string MediaFilePath)> GetMovieFoldersWithSingleMediaFile(string moviesRootPath)
        {
            var results = new List<(string, string)>();

            if (!Directory.Exists(moviesRootPath))
            {
                Logger.Error($"[SCAN ERROR] Movies root path does not exist: {moviesRootPath}");
                return results;
            }

            var subfolders = Directory.GetDirectories(moviesRootPath);

            foreach (var folder in subfolders)
            {
                try
                {
                    var mediaFiles = Directory
                        .GetFiles(folder)
                        .Where(file => ValidExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    if (mediaFiles.Count == 1)
                    {
                        results.Add((folder, mediaFiles[0]));
                        Logger.Info($"[SCAN CANDIDATE] Folder: {folder} | File: {mediaFiles[0]}");
                    }
                    else if (mediaFiles.Count == 0)
                    {
                        Logger.Debug($"[SCAN SKIP] Folder: {folder} | No valid media files found.");
                    }
                    else
                    {
                        Logger.Warning($"[SCAN SKIP] Folder: {folder} | Multiple media files found: {string.Join(", ", mediaFiles)}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[SCAN EXCEPTION] Error scanning folder: {folder} | {ex.Message}", ex);
                }
            }

            Logger.Info($"[SCAN COMPLETE] {results.Count} valid movie folders identified in {moviesRootPath}.");
            return results;
        }
    }
}
