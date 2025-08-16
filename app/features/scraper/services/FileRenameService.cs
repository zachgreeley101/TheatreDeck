using System;
using System.IO;
using theatredeck.app.core.logger;

namespace theatredeck.app.features.scraper.services
{
    /// <summary>
    /// Handles renaming of media files to append the Notion database ID.
    /// </summary>
    internal class FileRenameService
    {
        /// <summary>
        /// Renames the media file to append the database ID in the format [ULT-XXX] before the file extension.
        /// Returns the new file path if successful.
        /// Throws exceptions on failure (caller should handle and log).
        /// </summary>
        /// <param name="originalFilePath">Full path to the original media file.</param>
        /// <param name="databaseId">The database ID (e.g., "ULT-123") to append.</param>
        /// <returns>The full path of the renamed file.</returns>
        public static string RenameFileWithDatabaseId(string originalFilePath, string databaseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originalFilePath))
                {
                    Logger.Error("[RENAME ERROR] Original file path is null or empty.");
                    throw new ArgumentException("Original file path is null or empty.", nameof(originalFilePath));
                }
                if (string.IsNullOrWhiteSpace(databaseId))
                {
                    Logger.Error("[RENAME ERROR] Database ID is null or empty.");
                    throw new ArgumentException("Database ID is null or empty.", nameof(databaseId));
                }
                if (!File.Exists(originalFilePath))
                {
                    Logger.Error($"[RENAME ERROR] File not found: {originalFilePath}");
                    throw new FileNotFoundException("File not found.", originalFilePath);
                }

                string directory = Path.GetDirectoryName(originalFilePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
                string extension = Path.GetExtension(originalFilePath);

                // Remove any old [ULT-xxx] ID from filename
                string cleanFileName = System.Text.RegularExpressions.Regex.Replace(
                    fileNameWithoutExt, @"\s*\[ULT-[^\]]+\]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // UPDATED: Always include " - " before the branded ID
                string newFileName = $"{cleanFileName} - [{databaseId}]{extension}";
                string newFilePath = Path.Combine(directory, newFileName);

                if (File.Exists(newFilePath))
                {
                    Logger.Warning($"[RENAME ERROR] Target file already exists: {newFilePath}");
                    throw new IOException($"Cannot rename: target file '{newFilePath}' already exists.");
                }

                File.Move(originalFilePath, newFilePath);

                Logger.Info($"[RENAME SUCCESS] {originalFilePath} → {newFilePath}");
                return newFilePath;
            }
            catch (Exception ex)
            {
                Logger.Error($"[RENAME EXCEPTION] {originalFilePath} | ID: {databaseId} | {ex.Message}", ex);
                throw;
            }
        }
    }
}
