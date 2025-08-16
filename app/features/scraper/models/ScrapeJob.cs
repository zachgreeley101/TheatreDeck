using System;
using System.Collections.Generic;

namespace theatredeck.app.features.scraper.models
{
    /// <summary>
    /// Represents a scraping session/job for a specific media drive or folder.
    /// Tracks progress, processed files, new entries, skips, and errors.
    /// </summary>
    internal class ScrapeJob
    {
        /// <summary>
        /// The name or label of the server/media drive (e.g., "Media Drive 2").
        /// </summary>
        public string ServerDrive { get; set; }

        /// <summary>
        /// The root path being scraped (e.g., "Media Drive 2/Movies_2/").
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// The total number of movie folders/files identified to process.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// The number of files processed so far (for progress tracking).
        /// </summary>
        public int FilesProcessed { get; set; }

        /// <summary>
        /// List of files that were new and successfully added to Notion.
        /// </summary>
        public List<MediaFileInfo> NewEntries { get; set; } = new();

        /// <summary>
        /// List of files that already had a Database ID (and were skipped).
        /// </summary>
        public List<MediaFileInfo> SkippedFiles { get; set; } = new();

        /// <summary>
        /// List of errors (file path + error message).
        /// </summary>
        public List<(string FilePath, string ErrorMessage)> Errors { get; set; } = new();

        /// <summary>
        /// When the job started.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// When the job finished (null if still running).
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Calculates job duration if complete.
        /// </summary>
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

        /// <summary>
        /// Returns true if the scrape job is complete.
        /// </summary>
        public bool IsComplete => EndTime.HasValue;

        /// <summary>
        /// Optionally, a general status message for the UI.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Marks the job as finished and sets EndTime.
        /// </summary>
        public void Complete()
        {
            EndTime = DateTime.Now;
        }
    }
}
