using System.Configuration;
using System.Diagnostics;
using System.Text.Json;
using theatredeck.app.core.api.obs.services;
using theatredeck.app.core.logger;

namespace theatredeck.app.core.api.obs.utils
{
    public static class OBS_Helper
    {
        //===========================================
        // Action Helpers
        //===========================================
        /// <summary>
        /// Resets the track label time remaining in OBS by writing "0" to the designated file.
        /// </summary>
        public static void ResetTrackLabelTimeRemaining()
        {
            WriteTrackLabelTimeRemaining(0);
        }
        /// <summary>
        /// Clears and writes updated track information to the OBS label files.
        /// </summary>
        /// <param name="trackName">The track title.</param>
        /// <param name="dateReleased">The track release date.</param>
        /// <param name="artistPrimary">The primary artist name.</param>
        /// <param name="artistSecondary">The secondary artist name (or null for default "N/A").</param>
        public static void UpdateOBSLabels(string trackName, string dateReleased, string artistPrimary, string artistSecondary)
        {
            // Retrieve the base path from app.config
            string basePath = ConfigurationManager.AppSettings["OBSLibraryPath"];

            // Ensure the base path is configured
            if (string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("OBSLibraryPath is not configured in app.config.");

            // Compose the full directory path where the OBS label files reside
            string fullPath = Path.Combine(basePath, "assets", "scripts", "data", "trackinfo");

            // Define the file paths for each label
            string titleFilePath = Path.Combine(fullPath, "TrackLabelTitle.txt");
            string dateFilePath = Path.Combine(fullPath, "TrackLabelDate.txt");
            string artistPrimaryFilePath = Path.Combine(fullPath, "TrackLabelArtistPrimary.txt");
            string artistSecondaryFilePath = Path.Combine(fullPath, "TrackLabelArtistSecondary.txt");

            try
            {
                // Clear and update each file
                File.WriteAllText(titleFilePath, string.Empty);
                File.WriteAllText(titleFilePath, trackName);

                File.WriteAllText(dateFilePath, string.Empty);
                File.WriteAllText(dateFilePath, dateReleased);

                File.WriteAllText(artistPrimaryFilePath, string.Empty);
                File.WriteAllText(artistPrimaryFilePath, artistPrimary);

                File.WriteAllText(artistSecondaryFilePath, string.Empty);
                File.WriteAllText(artistSecondaryFilePath, artistSecondary ?? "N/A");

                Logger.Info($"[OBSLabelManager] Track info files updated successfully at {fullPath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[OBSLabelManager] Failed to update files: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Writes the adjusted track label time remaining to the designated file.
        /// </summary>
        /// <param name="adjustedSeconds">The adjusted total seconds to write.</param>
        public static void WriteTrackLabelTimeRemaining(double adjustedSeconds)
        {
            // Retrieve the base file path from app.config
            string basePath = ConfigurationManager.AppSettings["OBSLibraryPath"];

            if (string.IsNullOrWhiteSpace(basePath))
            {
                Logger.Warning("OBSLibraryPath is not configured in app.config.");
                return;
            }

            // Compose the full file path
            string filePath = Path.Combine(basePath, "assets", "scripts", "data", "trackinfo", "TrackLabelTimeRemaining.txt");

            try
            {
                // Write the adjusted seconds to the file
                File.WriteAllText(filePath, adjustedSeconds.ToString());
                Logger.Info($"[OBSFileManager] Updated track label time remaining to {adjustedSeconds} seconds in {filePath}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[OBSFileManager] Failed to update file: {ex.Message}", ex);
            }
        }














        //===========================================
        // Utility Helpers
        //===========================================
        public static async Task<int> RetryGetSceneItemId(
            OBSConnection clientConnection,
            string connectionType,
            string sceneName,
            string sourceName,
            CancellationToken cancellationToken = default)
        {
            const int maxRetries = 5;
            int retryCount = 0;
            int delayMilliseconds = 500; // initial delay

            while (retryCount < maxRetries)
            {
                try
                {
                    var sceneItemId = await GetSceneItemId(clientConnection, connectionType, sceneName, sourceName, cancellationToken);
                    return sceneItemId;
                }
                catch (KeyNotFoundException ex)
                {
                    Logger.Warning($"[RetryGetSceneItemId] Attempt {retryCount + 1}/{maxRetries} failed: {ex.Message}", ex);
                    retryCount++;
                    await Task.Delay(delayMilliseconds, cancellationToken);
                    delayMilliseconds *= 2; // exponential backoff
                }
            }

            throw new KeyNotFoundException($"Failed to retrieve SceneItemId for source '{sourceName}' after {maxRetries} retries.");
        }

        public static async Task<int> GetSceneItemId(
            OBSConnection clientConnection,
            string connectionType,
            string sceneName,
            string sourceName,
            CancellationToken cancellationToken = default)
        {
            var webSocket = clientConnection._webSockets[connectionType];

            // Generate a unique requestId for this call.
            string requestId = Guid.NewGuid().ToString();

            var payload = new
            {
                op = 6,
                d = new
                {
                    requestType = "GetSceneItemId",
                    requestId,
                    requestData = new
                    {
                        sceneName,
                        sourceName
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            await clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

            // Loop until we get the response matching our requestId.
            while (true)
            {
                string response = await clientConnection.ReceiveMessageAsync(webSocket, cancellationToken);
                try
                {
                    var document = JsonDocument.Parse(response);
                    if (document.RootElement.TryGetProperty("d", out JsonElement dElement) &&
                        dElement.TryGetProperty("requestId", out JsonElement respRequestId) &&
                        respRequestId.GetString() == requestId)
                    {
                        if (dElement.TryGetProperty("responseData", out JsonElement responseData) &&
                            responseData.TryGetProperty("sceneItemId", out JsonElement sceneItemId))
                        {
                            return sceneItemId.GetInt32();
                        }
                        else
                        {
                            throw new KeyNotFoundException("SceneItemId not found in the response.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GetSceneItemId] Error parsing response: {ex.Message}. Raw response: {response}", ex);
                }
            }
        }

    }
}