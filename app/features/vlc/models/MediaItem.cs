using System;
using theatredeck.app.core.api.notion.models; // For NotionDatabaseModel

namespace theatredeck.app.features.vlc.models
{
    /// <summary>
    /// Represents a single media item (audio or video) for VLC playback.
    /// </summary>
    public class MediaItem
    {
        /// <summary>
        /// Gets or sets the full file path to the media item.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the display name for this media item (parsed from filename or metadata).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the duration of the media item (if available).
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the media item's artwork image file path (optional).
        /// </summary>
        public string ArtworkPath { get; set; }

        /// <summary>
        /// Gets or sets the start time (in seconds) from which to begin playback.
        /// </summary>
        public int StartTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the end time (in seconds) at which to stop playback.
        /// </summary>
        public int EndTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the date this item was added to the playlist/library.
        /// </summary>
        public DateTime? DateAdded { get; set; }

        /// <summary>
        /// Gets or sets a user-defined tag or category for this media item.
        /// </summary>
        public string Tag { get; set; }

        // --- NEW: Notion metadata fields ---

        /// <summary>
        /// Holds the Notion page data associated with this media item (if any).
        /// </summary>
        public NotionDatabaseModel NotionData { get; set; }

        /// <summary>
        /// True if Notion metadata has been successfully loaded for this item.
        /// </summary>
        public bool NotionLoaded { get; set; }

        /// <summary>
        /// Stores any error encountered during Notion lookup.
        /// </summary>
        public Exception NotionLoadError { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaItem"/> class.
        /// </summary>
        public MediaItem() { }

        /// <summary>
        /// Convenience constructor for essential fields.
        /// </summary>
        public MediaItem(
            string filePath,
            string displayName,
            TimeSpan? duration = null,
            string artworkPath = null,
            DateTime? dateAdded = null,
            string tag = null,
            int startTimeSeconds = 0,
            int endTimeSeconds = 0)
        {
            FilePath = filePath;
            DisplayName = displayName;
            Duration = duration;
            ArtworkPath = artworkPath;
            DateAdded = dateAdded;
            Tag = tag;
            StartTimeSeconds = startTimeSeconds;
            EndTimeSeconds = endTimeSeconds;
            // Notion fields are left null/default
        }

        /// <summary>
        /// Returns the display name or filename as string representation.
        /// </summary>
        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(DisplayName)
                ? DisplayName
                : System.IO.Path.GetFileName(FilePath);
        }
    }
}
