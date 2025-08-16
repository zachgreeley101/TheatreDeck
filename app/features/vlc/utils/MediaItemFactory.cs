using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using theatredeck.app.features.vlc.models;
using theatredeck.app.core.api.notion;
using theatredeck.app.core.api.notion.models;
using System.Windows.Forms; // Needed for Invoke (if used in WinForms)

namespace theatredeck.app.features.vlc.utils
{
    internal static class MediaItemFactory
    {
        /// <summary>
        /// Creates a MediaItem, parses for Notion ID, and fetches Notion data if available.
        /// Captures tags and Notion PageId for updating purposes.
        /// Optionally accepts a callback to notify when Notion data is loaded.
        /// </summary>
        /// <param name="filePath">The file path of the media item.</param>
        /// <param name="notionManager">An instance of NotionManager to fetch Notion data.</param>
        /// <param name="onNotionLoaded">An optional callback invoked after Notion data is loaded into the MediaItem (will be called on UI thread if control provided).</param>
        /// <param name="uiControl">Optional WinForms control for Invoke (e.g. the Form or a Label); required if callback updates UI controls.</param>
        /// <returns>A fully populated MediaItem with Notion data, tags, and PageId if available.</returns>
        public static async Task<MediaItem> CreateMediaItemWithNotionDataAsync(
            string filePath,
            NotionManager notionManager,
            Action<MediaItem> onNotionLoaded = null, // Optional callback
            Control uiControl = null // Optional UI control for thread-safe update
        )
        {
            // Parse file name for display name
            string displayName = FilenameParser.Parse(filePath);

            // Create basic media item
            var item = new MediaItem
            {
                FilePath = filePath,
                DisplayName = displayName,
                DateAdded = DateTime.Now
                // StartTimeSeconds and EndTimeSeconds will be set from Notion if found
            };

            // Helper to invoke the callback on the UI thread if needed
            void SafeInvokeCallback()
            {
                if (onNotionLoaded == null)
                    return;

                if (uiControl != null && uiControl.InvokeRequired)
                    uiControl.Invoke(new Action(() => onNotionLoaded(item)));
                else
                    onNotionLoaded(item);
            }

            // Try to extract Notion ID
            string notionId = FilenameParser.ExtractNotionId(filePath);
            if (!string.IsNullOrWhiteSpace(notionId))
            {
                try
                {
                    var placeholders = new Dictionary<string, string> { ["ID"] = notionId };
                    var notionPages = await notionManager.QueryNotionDatabaseAsync("QueryFilterbyID.json", placeholders);
                    var notionData = notionPages?.FirstOrDefault();

                    if (notionData != null)
                    {
                        item.NotionData = notionData;
                        item.NotionLoaded = true;

                        // Apply Notion metadata to media item
                        item.StartTimeSeconds = notionData.StartTime ?? 0;
                        item.EndTimeSeconds = notionData.EndTime ?? 0;

                        // Set display name as "Title (Year)" if Notion data is present
                        item.DisplayName = FormatTitleWithYear(notionData.Name, notionData.Year);

                        SafeInvokeCallback();
                    }
                    else
                    {
                        item.NotionLoaded = false;
                        SafeInvokeCallback();
                    }
                }
                catch (Exception ex)
                {
                    item.NotionLoaded = false;
                    item.NotionLoadError = ex;
                    SafeInvokeCallback();
                }
            }
            else
            {
                // No Notion ID found, invoke callback anyway to update UI with basic item
                SafeInvokeCallback();
            }

            // If Notion data is not loaded, Start/End times may be set with default/fallback logic elsewhere.

            return item;
        }

        /// <summary>
        /// Helper method to format the title with the year as "Title (Year)".
        /// If year is missing, returns just the title.
        /// </summary>
        private static string FormatTitleWithYear(string title, int? year)
        {
            if (string.IsNullOrWhiteSpace(title))
                title = "Untitled";

            if (year.HasValue && year.Value > 0)
                return $"{title.Trim()} ({year.Value})";
            else
                return title.Trim();
        }
    }
}
