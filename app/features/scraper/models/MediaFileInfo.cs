using System;

namespace theatredeck.app.features.scraper.models
{
    /// <summary>
    /// Represents metadata and file information parsed from a movie media file and its folder.
    /// </summary>
    internal class MediaFileInfo
    {
        /// <summary>
        /// Full path to the folder containing the media file.
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// Full path to the media file itself.
        /// </summary>
        public string MediaFilePath { get; set; }

        /// <summary>
        /// Movie title, parsed from the file or folder name.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Year of release, parsed from the file or folder name.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// File extension (e.g., "mkv", "mp4").
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Database ID found in file name (e.g., "ULT-123"), or null if new.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Original file name (with extension).
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Indicates whether the file is already registered in the database.
        /// </summary>
        public bool HasDatabaseId => !string.IsNullOrWhiteSpace(DatabaseId);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MediaFileInfo() { }

        /// <summary>
        /// Convenience constructor for fast initialization.
        /// </summary>
        public MediaFileInfo(
            string folderPath,
            string mediaFilePath,
            string title,
            int? year,
            string extension,
            string databaseId,
            string originalFileName)
        {
            FolderPath = folderPath;
            MediaFilePath = mediaFilePath;
            Title = title;
            Year = year;
            Extension = extension;
            DatabaseId = databaseId;
            OriginalFileName = originalFileName;
        }
    }
}
