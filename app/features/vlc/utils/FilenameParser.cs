using System;
using System.IO;
using System.Text.RegularExpressions;

namespace theatredeck.app.features.vlc.utils
{
    /// <summary>
    /// Provides static helper methods for parsing and cleaning media filenames.
    /// Always returns "Title (Year)" if possible, cutting off extra tags after the year.
    /// </summary>
    public static class FilenameParser
    {
        /// <summary>
        /// Parses a filename and returns a display-friendly media title:
        /// everything up to and including the last (YYYY) year in parentheses.
        /// Example: "A Bad Moms Christmas (2017) Bluray-1080p.mkv" → "A Bad Moms Christmas (2017)"
        /// </summary>
        public static string Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            // Get file name without extension
            string filename = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrWhiteSpace(filename))
                return string.Empty;

            // Replace common separators (dot/underscore) with spaces
            filename = filename.Replace('.', ' ').Replace('_', ' ');

            // Collapse multiple spaces
            filename = Regex.Replace(filename, @"\s{2,}", " ").Trim();

            // Find the last (YYYY) and keep up to that, if it exists
            var lastParenYear = Regex.Match(filename, @"\((19|20)\d{2}\)(?!.*\((19|20)\d{2}\))");
            if (lastParenYear.Success)
            {
                int endIdx = lastParenYear.Index + lastParenYear.Length;
                filename = filename.Substring(0, endIdx).Trim();
            }

            // Final space collapse
            filename = Regex.Replace(filename, @"\s{2,}", " ").Trim();

            // Title-case for UI consistency
            filename = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(filename.ToLower());

            return filename;
        }
        /// <summary>
        /// Extracts a Notion ID of the format [ULT-<number>] at the end of the filename (before extension).
        /// Returns the ID string (e.g., ULT-20), or null if not found.
        /// </summary>
        public static string ExtractNotionId(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            // Get the file name without extension
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Strict regex: match [ULT-<number>] at end of fileName
            var match = Regex.Match(fileName, @"\[ULT-(\d+)\]$");

            if (match.Success)
            {
                // Return just the ID, not including brackets
                return $"ULT-{match.Groups[1].Value}";
            }

            // No valid ID found
            return null;
        }
    }
}
