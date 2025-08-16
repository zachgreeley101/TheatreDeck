using System;
using System.IO;
using System.Text.RegularExpressions;

namespace theatredeck.app.features.scraper.utils
{
    /// <summary>
    /// Utility methods for parsing movie metadata from file and folder names.
    /// </summary>
    internal static class MediaParser
    {
        // Pattern for [ULT-###] ID in filename
        private static readonly Regex DatabaseIdRegex = new(@"\[ULT-(\w+)\]", RegexOptions.Compiled);

        // Pattern for (YYYY) year in filename or folder name
        private static readonly Regex YearRegex = new(@"\((19|20)\d{2}\)", RegexOptions.Compiled);

        /// <summary>
        /// Attempts to extract the database ID (ULT format) from the file name.
        /// </summary>
        public static string ExtractDatabaseId(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var match = DatabaseIdRegex.Match(fileName);
            return match.Success ? $"ULT-{match.Groups[1].Value}" : null;
        }

        /// <summary>
        /// Attempts to extract the year from file name or folder name.
        /// </summary>
        public static int? ExtractYear(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var match = YearRegex.Match(name);
            if (match.Success && int.TryParse(match.Value.Trim('(', ')'), out int year))
                return year;

            return null;
        }

        /// <summary>
        /// Attempts to extract the movie title from the file name (removes year and ID).
        /// </summary>
        public static string ExtractTitle(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            // Remove [ULT-###] if present
            nameWithoutExt = DatabaseIdRegex.Replace(nameWithoutExt, "");
            // Remove (YYYY) if present
            nameWithoutExt = YearRegex.Replace(nameWithoutExt, "");
            // Clean up spaces and dashes
            return nameWithoutExt.Trim().Trim('-', '_');
        }

        /// <summary>
        /// Gets the file extension (without the dot).
        /// </summary>
        public static string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath)?.TrimStart('.').ToLower();
        }
    }
}
