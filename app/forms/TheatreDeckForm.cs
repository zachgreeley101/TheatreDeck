using System.Configuration;
using System.Diagnostics;
using theatredeck.app.core.api.notion;
using theatredeck.app.core.api.notion.models;
using theatredeck.app.core.api.obs;
using theatredeck.app.core.api.obs.utils;
using theatredeck.app.core.config;
using theatredeck.app.core.logger;
using theatredeck.app.core.utils;
using theatredeck.app.features.obs.events;
using theatredeck.app.features.scraper;
using theatredeck.app.features.vlc;
using theatredeck.app.features.vlc.controllers;
using theatredeck.app.features.vlc.interfaces;
using theatredeck.app.features.vlc.models;
using theatredeck.app.features.vlc.utils;

namespace theatredeck.app.forms
{
    public partial class TheatreDeckForm : Form, IEventSubscriberVLC
    {
        public NotionManager _notionManager { get; private set; }
        public OBSManager _obsManager;
        private CancellationTokenSource _cancellationTokenSource;
        public bool IsInitializingDefaults { get; set; } = false;
        public static TheatreDeckForm Instance { get; private set; }
        private PlaybackController _vlcPlaybackController;
        private FeatureBootstrapperVLC _vlcBootstrapper;
        private System.Threading.CancellationTokenSource _vlcConnectCts;
        private bool _vlcConnected = false;
        private PlaybackState _lastLoggedPlaybackState = null;

        //======================================
        // Form Initialization & Closing
        //======================================
        public TheatreDeckForm()
        {
            InitializeComponent();

            // Initialize _notionManager
            _notionManager = new NotionManager(new HttpClient());

            // Initialize OBS Manager
            _obsManager = new OBSManager();

            // Initialize VLC HTTP
            _vlcBootstrapper = new FeatureBootstrapperVLC(_notionManager);
            _vlcBootstrapper.RegisterSubscriber(this);
            _vlcPlaybackController = _vlcBootstrapper.PlaybackController;

            Instance = this;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Disable Set buttons at startup
            SetTagButtonsEnabled(false);

            // Start monitoring VLC connection
            StartVlcConnectionMonitor();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _notionManager?.Dispose();
            _vlcConnectCts?.Cancel();
            _vlcConnectCts?.Dispose();
            _vlcPlaybackController.StopPollingVlcStatus();
            NowNextStatusWriter.WriteStopToTimeFiles();
            base.OnFormClosing(e);

        }

        //======================================
        // Tool Strip Menu - Top Level
        //======================================

        // Tools > OBS Controls
        private void launchScreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OBSLauncher.LaunchOBS("Screen");
        }
        private void launchCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OBSLauncher.LaunchOBS("Camera");
        }
        private void autoLaunchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OBSLauncher.LaunchOBS("Screen");
            OBSLauncher.LaunchOBS("Camera");
        }

        // Test > Notion
        private async void queryDatabaseSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _notionManager.QueryAndSaveDatabaseSchemaAsync();
        }
        private async void queryDatabaseByFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug("BtnQueryNotionFilter_Click triggered.");

                // Call the method to query the database using your filter template
                var models = await _notionManager.QueryNotionDatabaseAsync("QueryFilterbyID.json");

                Logger.Info($"Query execution completed successfully. Results returned: {models.Count}");

                MessageBox.Show($"Database query completed successfully.\nResults returned: {models.Count}",
                                "Query Successful",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error querying Notion database: {ex.Message}", ex);
                MessageBox.Show($"Error querying Notion database: {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private async void updatePagePropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Define the page ID and the updates
                string pageId = "2328d0ed8efc8062a5a8cce75565915d";
                var propertyUpdates = new Dictionary<string, object>
        {
            { ConfigurationManager.AppSettings["Creation_Notion"], true }
        };

                // Call the UpdateNotionPageAsync method
                await _notionManager.UpdateNotionPagePropertyAsync(pageId, propertyUpdates);

                // Optional: Display success message
                MessageBox.Show("The 'Creation' property was successfully updated to 'false'.",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle any errors and display a message box
                MessageBox.Show($"An error occurred while updating the Notion page:\n{ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        private async void createNewPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var httpClient = new HttpClient();
            var NotionManager = new NotionManager(httpClient);

            // Populate the NotionNewPageModel with sample data for your new schema
            var notionData = new NotionNewPageModel
            {
                ServerDrive = new List<string> { "D1", "D2" },
                Tags = new List<string> { "Soundtrack", "Synthwave" },
                Collections = new List<string> { "MainStage", "Archive" },
                PlayNext = "false",   // Use "true" or "false" as string; change to bool if preferred
                Year = 2024,
                Volume = 100,
                PlayCount = 5,
                StartTime = 0,
                SkippingTime = "Intro",
                EndTime = 210,
                Location = "/Example/Path/Location",
                Name = "Example Page"
            };

            try
            {
                // Call the CreateNotionPageAsync method
                string response = await NotionManager.CreateNotionPageAsync(notionData);

                // Display the response in a message box
                MessageBox.Show("Notion Page Created Successfully:\n" + response, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Display error message
                MessageBox.Show("An error occurred while creating the Notion page:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Test > Buttons
        private async void testButton1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string connectionType = "Camera"; // or "Camera" depending on your setup
            string sceneName = "Main";   // Replace with your actual scene name
            string sourceName = "Image";    // Replace with the actual source name

            // Call the ToggleSourceVisibility method asynchronously
            await _obsManager.ToggleSourceVisibility(connectionType, sceneName, sourceName);
        }
        private async void testButton2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Hardcoded Notion page ID
                string pageId = "1808d0ed8efc810aaab0c846c19ef2ce"; // Replace with your actual test page ID

                // Define a list of tags
                var tagList = new List<string> { "TestTag1", "TestTag2", "TestTag3" };

                // Get the Tags property ID from config
                string tagsPropertyId = ConfigurationManager.AppSettings["Tags_Notion"];
                if (string.IsNullOrEmpty(tagsPropertyId))
                {
                    MessageBox.Show("Tags_Notion is not configured in App.config.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Build the dictionary with formatted multi_select property
                var propertyUpdates = new Dictionary<string, object>
                {
                    [tagsPropertyId] = tagList
                };

                // Call Notion update method
                await _notionManager.UpdateNotionPagePropertyAsync(pageId, propertyUpdates);

                MessageBox.Show("Multi-select Tags updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating tags: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //======================================
        // OBS Buttons
        //======================================
        private async void BtnObsTest_Click(object sender, EventArgs e)
        {
            var OBSTempEvent = new TempEvents(_obsManager);
            await OBSTempEvent.ExecuteAsync();
        }
        private async void checkBoxLavaLamps_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLavaLamps.Checked)
            {
                await _obsManager.EnableSourceVisibility("Screen", "Main", "Lava Lamps");
            }
            else
            {
                await _obsManager.DisableSourceVisibility("Screen", "Main", "Lava Lamps");
            }
        }
        private void comboBoxOperationMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsInitializingDefaults) return;

            if (sender is ComboBox cb && cb.SelectedItem != null)
            {
                var selected = cb.SelectedItem.ToString();

                ConfigManager.SetAppSetting("CBDefaultOperationMode", selected);
                Logger.Info($"[ComboBox Update] OperationMode = {ConfigManager.GetAppSettingValue("CBDefaultOperationMode")}");
            }
        }


        //======================================
        // New Controls for VLC
        //======================================
        private void btnRefreshStatus_Click(object sender, EventArgs e)
        {
            Logger.Info("[MainForm] RefreshStatus button clicked.");
            var state = _vlcPlaybackController.GetCurrentState();
            lblNowPlaying.Text = state.CurrentMedia?.DisplayName ?? "No media";
            RefreshPlaylistUI();
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            Logger.Info("[MainForm] Pause button clicked.");
            _vlcPlaybackController.Pause();
        }
        private async void bntBrowseAdd_Click(object sender, EventArgs e)
        {
            Logger.Info("[MainForm] Browse/Add button clicked.");
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Media Files|*.mp4;*.mp3;*.mkv;*.avi;*.flac;*.wav";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                    {
                        if (_vlcPlaybackController.ContainsMedia(file))
                        {
                            Logger.Info($"[MainForm] Skipped duplicate media: {file}");
                            continue; // Skip duplicate files
                        }

                        // Use factory to create MediaItem with Notion data, provide callback for UI update
                        var item = await MediaItemFactory.CreateMediaItemWithNotionDataAsync(
                            file,
                            _notionManager,
                            onNotionLoaded: (loadedItem) => RefreshPlaylistUI(),
                            uiControl: this
                        );

                        // If Notion data wasn't found, fall back to default timing
                        if (!item.NotionLoaded)
                        {
                            var (start, end) = MediaTimingProvider.GetTimingFor(file);
                            item.StartTimeSeconds = start;
                            item.EndTimeSeconds = end;
                        }

                        _vlcPlaybackController.AddMedia(item);
                    }
                    RefreshPlaylistUI();
                }
            }
        }
        private void btnRemoveFromPlaylist_Click(object sender, EventArgs e)
        {
            Logger.Info($"[MainForm] btnRemoveFromPlaylist_Click invoked, SelectedIndex(before)={lstPlaylist.SelectedIndex}, ItemCount(before)={lstPlaylist.Items.Count}");

            int selectedIdx = lstPlaylist.SelectedIndex;
            if (selectedIdx < 0)
            {
                Logger.Info("[MainForm] btnRemoveFromPlaylist_Click skipped: No item selected.");
                return;
            }

            // Map selected string (may have "> ") back to playlist MediaItem
            var playlist = _vlcPlaybackController.GetPlaylist();
            string selectedDisplay = lstPlaylist.Items[selectedIdx]?.ToString();
            if (selectedDisplay.StartsWith("> ")) selectedDisplay = selectedDisplay.Substring(2);

            MediaItem selectedItem = playlist.FirstOrDefault(item => item.DisplayName == selectedDisplay);
            if (selectedItem == null)
            {
                Logger.Info("[MainForm] btnRemoveFromPlaylist_Click skipped: No matching MediaItem found.");
                return;
            }

            // Use LINQ to find the index, since FindIndex is not available on IList<>
            int removedIndex = playlist
                .Select((item, index) => new { item, index })
                .FirstOrDefault(x => x.item == selectedItem)?.index ?? -1;
            int currentPlayingIndex = _vlcPlaybackController.CurrentPlaylistIndex;

            Logger.Info($"[MainForm] Preparing to remove item at playlist index={removedIndex} ('{selectedItem.DisplayName}'). CurrentPlayingIndex={currentPlayingIndex}");

            _vlcPlaybackController.RemoveMedia(selectedItem);

            // Log state after removal at the controller level
            Logger.Info($"[MainForm] After RemoveMedia: CurrentPlaylistIndex={_vlcPlaybackController.CurrentPlaylistIndex}");

            // If the removed item was the currently playing one, advance playback
            if (removedIndex == currentPlayingIndex)
            {
                Logger.Info("[MainForm] Removed item was the currently playing item. Attempting to advance playback.");
                _vlcPlaybackController.AdvanceToNextMedia();
            }
            else
            {
                Logger.Info("[MainForm] Removed item was not the currently playing item. No advance needed.");
            }

            RefreshPlaylistUI();

            // Adjust selection so the user isn't left with nothing selected
            if (lstPlaylist.Items.Count > 0)
            {
                int newIndex = Math.Min(lstPlaylist.Items.Count - 1, removedIndex);
                lstPlaylist.SelectedIndex = newIndex;
                Logger.Info($"[MainForm] Playlist not empty after removal. Setting SelectedIndex to {newIndex}.");
            }

            // If playlist is empty after removal, update UI accordingly
            if (lstPlaylist.Items.Count == 0)
            {
                Logger.Info("[MainForm] Playlist is now empty after removal. Updating UI for empty state.");
                UpdateUIForEmptyPlaylist();
            }

            // === Updated: Update Now/Next Status after removal, always handle 1-item playlist ===
            var latestPlaylist = _vlcPlaybackController.GetPlaylist();
            int nowPlayingIdx = _vlcPlaybackController.CurrentPlaylistIndex;
            MediaItem now = (nowPlayingIdx >= 0 && nowPlayingIdx < latestPlaylist.Count) ? latestPlaylist[nowPlayingIdx] : null;
            MediaItem next = null;
            if (latestPlaylist.Count == 1)
            {
                next = now; // Loop: only one item, so next is also now
            }
            else if (latestPlaylist.Count > 1 && nowPlayingIdx >= 0)
            {
                int nextIdx = (nowPlayingIdx + 1) % latestPlaylist.Count;
                next = latestPlaylist[nextIdx];
            }
            NowNextStatusWriter.WriteNowAndNext(now, next);
        }
        private async void btnMoveUp_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Info($"[MainForm] btnMoveUp_Click invoked, SelectedIndex(before)={lstPlaylist.SelectedIndex}, ItemCount={lstPlaylist.Items.Count}");

                int selectedIdx = lstPlaylist.SelectedIndex;

                // Edge: No selection or already at top
                if (selectedIdx <= 0)
                {
                    Logger.Info($"[MainForm] btnMoveUp_Click skipped: invalid idx={selectedIdx}. (Must be >0 and within range)");
                    return;
                }

                // Map selected string (may have "> ") back to playlist index
                var playlist = _vlcPlaybackController.GetPlaylist();
                string selectedDisplay = lstPlaylist.Items[selectedIdx]?.ToString();
                if (selectedDisplay.StartsWith("> ")) selectedDisplay = selectedDisplay.Substring(2);

                // Use LINQ to find the index (replacement for .FindIndex)
                int playlistIdx = playlist
                    .Select((item, index) => new { item, index })
                    .FirstOrDefault(x => x.item.DisplayName == selectedDisplay)?.index ?? -1;

                if (playlistIdx <= 0)
                {
                    Logger.Info($"[MainForm] btnMoveUp_Click: mapped playlistIdx invalid ({playlistIdx}).");
                    return;
                }

                // Track unique FilePath for selection after move
                string movedFilePath = playlist[playlistIdx].FilePath;

                Logger.Info($"[MainForm] Preparing to move up item at playlistIdx={playlistIdx} ('{playlist[playlistIdx].DisplayName}')");

                // Move selected item up in the playlist (await async method)
                int newIndex = await _vlcPlaybackController.MoveMediaUp(playlistIdx);

                Logger.Info($"[MainForm] MoveMediaUp returned newIndex={newIndex}. Refreshing playlist UI.");

                RefreshPlaylistUI();

                // Reselect the item at its new position in the ListBox using FilePath (unique ID)
                var newPlaylist = _vlcPlaybackController.GetPlaylist();
                int movedListBoxIndex = -1;
                for (int i = 0; i < newPlaylist.Count; i++)
                {
                    if (newPlaylist[i].FilePath == movedFilePath)
                    {
                        // Build display name (with ">" if it's now playing)
                        string display = newPlaylist[i].DisplayName;
                        if (i == _vlcPlaybackController.CurrentPlaylistIndex)
                            display = $"> {display}";

                        // Now find the corresponding index in the ListBox
                        for (int j = 0; j < lstPlaylist.Items.Count; j++)
                        {
                            if (lstPlaylist.Items[j]?.ToString() == display)
                            {
                                movedListBoxIndex = j;
                                break;
                            }
                        }
                        break;
                    }
                }
                if (movedListBoxIndex != -1)
                {
                    lstPlaylist.SelectedIndex = movedListBoxIndex;
                    lstPlaylist.TopIndex = movedListBoxIndex;
                    Logger.Info($"[MainForm] After move up: SelectedIndex={lstPlaylist.SelectedIndex}, TopIndex={lstPlaylist.TopIndex}");
                }

                // === NEW: Update Now/Next Status after moving ===
                var latestPlaylist = _vlcPlaybackController.GetPlaylist();
                int nowPlayingIdx = _vlcPlaybackController.CurrentPlaylistIndex;
                MediaItem now = (nowPlayingIdx >= 0 && nowPlayingIdx < latestPlaylist.Count) ? latestPlaylist[nowPlayingIdx] : null;
                MediaItem next = null;
                if (latestPlaylist.Count > 1 && nowPlayingIdx >= 0)
                {
                    int nextIdx = (nowPlayingIdx + 1) % latestPlaylist.Count;
                    next = latestPlaylist[nextIdx];
                }
                NowNextStatusWriter.WriteNowAndNext(now, next);

            }
            finally
            {
                Logger.Info($"[MainForm] btnMoveUp_Click complete. SelectedIndex(after)={lstPlaylist.SelectedIndex}, ItemCount(after)={lstPlaylist.Items.Count}");
            }
        }
        private async void btnMoveDown_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Info($"[MainForm] btnMoveDown_Click invoked, SelectedIndex(before)={lstPlaylist.SelectedIndex}, ItemCount={lstPlaylist.Items.Count}");

                int selectedIdx = lstPlaylist.SelectedIndex;

                // Edge: No selection or already at bottom
                if (selectedIdx < 0 || selectedIdx >= lstPlaylist.Items.Count - 1)
                {
                    Logger.Info($"[MainForm] btnMoveDown_Click skipped: invalid idx={selectedIdx}. (Must be >=0 and less than last item)");
                    return;
                }

                // Map selected string (may have "> ") back to playlist index
                var playlist = _vlcPlaybackController.GetPlaylist();
                string selectedDisplay = lstPlaylist.Items[selectedIdx]?.ToString();
                if (selectedDisplay.StartsWith("> ")) selectedDisplay = selectedDisplay.Substring(2);

                // Use LINQ to find the index (replacement for .FindIndex)
                int playlistIdx = playlist
                    .Select((item, index) => new { item, index })
                    .FirstOrDefault(x => x.item.DisplayName == selectedDisplay)?.index ?? -1;

                if (playlistIdx < 0 || playlistIdx >= playlist.Count - 1)
                {
                    Logger.Info($"[MainForm] btnMoveDown_Click: mapped playlistIdx invalid ({playlistIdx}).");
                    return;
                }

                // Track unique FilePath for selection after move
                string movedFilePath = playlist[playlistIdx].FilePath;

                Logger.Info($"[MainForm] Preparing to move down item at playlistIdx={playlistIdx} ('{playlist[playlistIdx].DisplayName}')");

                // Move selected item down in the playlist (await async method)
                int newIndex = await _vlcPlaybackController.MoveMediaDown(playlistIdx);

                Logger.Info($"[MainForm] MoveMediaDown returned newIndex={newIndex}. Refreshing playlist UI.");

                RefreshPlaylistUI();

                // Reselect the item at its new position in the ListBox using FilePath (unique ID)
                var newPlaylist = _vlcPlaybackController.GetPlaylist();
                int movedListBoxIndex = -1;
                for (int i = 0; i < newPlaylist.Count; i++)
                {
                    if (newPlaylist[i].FilePath == movedFilePath)
                    {
                        // Build display name (with ">" if it's now playing)
                        string display = newPlaylist[i].DisplayName;
                        if (i == _vlcPlaybackController.CurrentPlaylistIndex)
                            display = $"> {display}";

                        // Now find the corresponding index in the ListBox
                        for (int j = 0; j < lstPlaylist.Items.Count; j++)
                        {
                            if (lstPlaylist.Items[j]?.ToString() == display)
                            {
                                movedListBoxIndex = j;
                                break;
                            }
                        }
                        break;
                    }
                }
                if (movedListBoxIndex != -1)
                {
                    lstPlaylist.SelectedIndex = movedListBoxIndex;
                    lstPlaylist.TopIndex = movedListBoxIndex;
                    Logger.Info($"[MainForm] After move down: SelectedIndex={lstPlaylist.SelectedIndex}, TopIndex={lstPlaylist.TopIndex}");
                }

                // === NEW: Update Now/Next Status after moving ===
                var latestPlaylist = _vlcPlaybackController.GetPlaylist();
                int nowPlayingIdx = _vlcPlaybackController.CurrentPlaylistIndex;
                MediaItem now = (nowPlayingIdx >= 0 && nowPlayingIdx < latestPlaylist.Count) ? latestPlaylist[nowPlayingIdx] : null;
                MediaItem next = null;
                if (latestPlaylist.Count > 1 && nowPlayingIdx >= 0)
                {
                    int nextIdx = (nowPlayingIdx + 1) % latestPlaylist.Count;
                    next = latestPlaylist[nextIdx];
                }
                NowNextStatusWriter.WriteNowAndNext(now, next);
            }
            finally
            {
                Logger.Info($"[MainForm] btnMoveDown_Click complete. SelectedIndex(after)={lstPlaylist.SelectedIndex}, ItemCount(after)={lstPlaylist.Items.Count}");
            }
        }
        private void lstPlaylist_DoubleClick(object sender, EventArgs e)
        {
            int selectedIdx = lstPlaylist.SelectedIndex;
            if (selectedIdx < 0)
                return;

            // Map selected string (may have "> ") back to playlist index
            var playlist = _vlcPlaybackController.GetPlaylist();
            string selectedDisplay = lstPlaylist.Items[selectedIdx]?.ToString();
            if (selectedDisplay.StartsWith("> ")) selectedDisplay = selectedDisplay.Substring(2);

            int playlistIdx = playlist.Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.DisplayName == selectedDisplay)?.index ?? -1;
            if (playlistIdx < 0)
                return;

            // Play using the controller so custom timing is respected
            _vlcPlaybackController.PlayMediaAtIndex(playlistIdx);
        }
        private void RefreshPlaylistUI()
        {
            int prevSelectedIndex = lstPlaylist.SelectedIndex; // Save current selection

            lstPlaylist.Items.Clear();

            var playlist = _vlcPlaybackController.GetPlaylist();
            int playingIndex = _vlcPlaybackController.CurrentPlaylistIndex;

            for (int i = 0; i < playlist.Count; i++)
            {
                string displayName = playlist[i].DisplayName;
                if (i == playingIndex)
                    displayName = $"> {displayName}";
                lstPlaylist.Items.Add(displayName);
            }

            // Edge case: Empty playlist
            if (lstPlaylist.Items.Count == 0)
            {
                UpdateUIForEmptyPlaylist();
                return;
            }

            // Optionally: preserve selection if it makes sense
            if (prevSelectedIndex >= 0 && prevSelectedIndex < lstPlaylist.Items.Count)
                lstPlaylist.SelectedIndex = prevSelectedIndex;
        }

        private async Task<bool> TryConnectToVlcAsync()
        {
            try
            {
                using (var http = new theatredeck.app.features.vlc.controllers.HttpService())
                {
                    string status = await http.GetStatusXmlAsync();
                    Logger.Info("[MainForm] VLC connection successful.");
                    return !string.IsNullOrWhiteSpace(status);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[MainForm] VLC connection failed.", ex);
                return false;
            }
        }
        public void OnPlaybackStateChanged(PlaybackState state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPlaybackStateChanged(state)));
                return;
            }

            if (state.State == EventType.Searching)
            {
                lblNowPlaying.Text = "Searching for VLC...";
                labCurrentTime.Text = "--:--:--";
                labTotalLength.Text = "--:--:--";
                labPlaybackState.Text = "Searching...";
                lblVolumePercent.Text = "--%";
                lstPlaylist.ClearSelected();

                SetUiEnabled(false);
                trackBarVolume.Enabled = false;
                SetTagButtonsEnabled(false);

                UpdateTagLabelsFromCurrentMedia(null);
                return;
            }

            SetUiEnabled(true);
            trackBarVolume.Enabled = true;

            if (lstPlaylist.Items.Count == 0 || state.CurrentMedia == null)
            {
                lblNowPlaying.Text = "No media";
                labCurrentTime.Text = "--:--:--";
                labTotalLength.Text = "--:--:--";
                labPlaybackState.Text = "Stopped";
                lstPlaylist.ClearSelected();

                SetTagButtonsEnabled(false);
                UpdateTagLabelsFromCurrentMedia(null);
                return;
            }

            lblNowPlaying.Text = state.CurrentMedia?.DisplayName ?? "No media";
            labCurrentTime.Text = state.Position.ToString(@"hh\:mm\:ss");
            labTotalLength.Text = state.Duration.HasValue
                ? state.Duration.Value.ToString(@"hh\:mm\:ss")
                : "--:--:--";
            labPlaybackState.Text = state.State.ToString();

            if (!trackBarVolume.Capture)
            {
                int vlcVolume = state.Volume;
                if (vlcVolume >= trackBarVolume.Minimum && vlcVolume <= trackBarVolume.Maximum)
                    trackBarVolume.Value = vlcVolume;
                lblVolumePercent.Text = $"{vlcVolume}%";
            }

            bool enableSetButtons = (state.CurrentMedia != null)
                && (state.State == EventType.Playing || state.State == EventType.Paused);
            SetTagButtonsEnabled(enableSetButtons);

            UpdateTagLabelsFromCurrentMedia(state.CurrentMedia);
            RefreshPlaylistUI();

            if (!string.IsNullOrWhiteSpace(state.ErrorMessage) || state.State == EventType.Error)
            {
                Logger.Error($"OnPlaybackStateChanged: ERROR - {state.ErrorMessage} | Media: {state.CurrentMedia} | State: {state.State}");
            }
        }
        private void UpdateUIForEmptyPlaylist()
        {
            lstPlaylist.ClearSelected();
            lblNowPlaying.Text = "No media";
            labCurrentTime.Text = "--:--:--";
            labTotalLength.Text = "--:--:--";
            labPlaybackState.Text = "Stopped";
            lblVolumePercent.Text = "--%";

            btnRemoveFromPlaylist.Enabled = false;
            btnMoveUp.Enabled = false;
            btnMoveDown.Enabled = false;

            Logger.Info("[MainForm] UI updated for empty playlist.");
        }
        public void OnPlaybackEvent(EventType eventType, PlaybackState state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPlaybackEvent(eventType, state)));
                return;
            }

            if (eventType == EventType.Error && !string.IsNullOrWhiteSpace(state.ErrorMessage))
            {
                Logger.Error($"[MainForm] Playback error: {state.ErrorMessage}");
                MessageBox.Show(state.ErrorMessage, "Playback Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void StartVlcConnectionMonitor()
        {
            Logger.Info("[MainForm] Starting VLC connection monitor (UI locked).");
            SetUiEnabled(false); // Lock UI initially

            // --- Notify UI that VLC is being searched for ---
            var searchingState = new PlaybackState
            {
                State = EventType.Searching, // Make sure this enum value exists!
                CurrentMedia = null,
                Position = TimeSpan.Zero,
                Duration = null,
                IsMuted = false,
                ErrorMessage = null,
                Volume = 0
            };
            // Manually update UI to reflect searching state
            OnPlaybackStateChanged(searchingState);

            _vlcConnectCts = new System.Threading.CancellationTokenSource();
            Task.Run(() => MonitorVlcConnectionLoop(_vlcConnectCts.Token));
        }
        private async Task MonitorVlcConnectionLoop(System.Threading.CancellationToken token)
        {
            Logger.Info("[MainForm] VLC connection monitor loop started.");

            while (!token.IsCancellationRequested && !_vlcConnected)
            {
                Logger.Info("[MainForm] Attempting to connect to VLC...");
                bool connected = await TryConnectToVlcAsync();
                if (connected)
                {
                    _vlcConnected = true;
                    Logger.Info("[MainForm] Connected to VLC. Unlocking UI.");

                    // Must update UI from the UI thread
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => SetUiEnabled(true)));
                    }
                    else
                    {
                        SetUiEnabled(true);
                    }

                    // Start polling VLC status after connection established
                    _vlcPlaybackController.StartPollingVlcStatus();

                    break;
                }

                Logger.Info("[MainForm] VLC connection attempt failed. Retrying in 5 seconds...");
                await Task.Delay(5000, token); // Wait 5 seconds before retrying
            }

            if (token.IsCancellationRequested)
                Logger.Info("[MainForm] VLC connection monitor loop cancelled.");
        }
        private void SetUiEnabled(bool enabled)
        {
            if (btnPause != null) btnPause.Enabled = enabled;
            if (btnRemoveFromPlaylist != null) btnRemoveFromPlaylist.Enabled = enabled;
            if (bntBrowseAdd != null) bntBrowseAdd.Enabled = enabled;
            if (btnMoveUp != null) btnMoveUp.Enabled = enabled;
            if (btnMoveDown != null) btnMoveDown.Enabled = enabled;
            if (lstPlaylist != null) lstPlaylist.Enabled = enabled;
            if (trackBarVolume != null) trackBarVolume.Enabled = enabled;
        }
        private async void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            int currentVolumePercent = trackBarVolume.Value;
            lblVolumePercent.Text = $"{currentVolumePercent}%";
            await _vlcPlaybackController.SetVolumeAsync(currentVolumePercent);
        }
        private void launchVLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Standard Windows VLC installation path
                string vlcPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";

                if (!System.IO.File.Exists(vlcPath))
                {
                    // Try Program Files (x86) for 32-bit installs on 64-bit Windows
                    vlcPath = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
                }

                if (!System.IO.File.Exists(vlcPath))
                {
                    MessageBox.Show(
                        "VLC media player was not found in the standard installation locations.\n" +
                        "Please install VLC or configure its path.",
                        "VLC Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // Launch VLC
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = vlcPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch VLC media player:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void exitVLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var vlcProcesses = System.Diagnostics.Process.GetProcessesByName("vlc");
                if (vlcProcesses.Length == 0)
                {
                    MessageBox.Show(
                        "No running VLC process was found.",
                        "VLC Not Running",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                foreach (var process in vlcProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(2000); // Optionally wait up to 2 seconds for each process to exit
                    }
                    catch (Exception killEx)
                    {
                        // If one instance fails to kill, continue with others
                        MessageBox.Show(
                            $"Failed to terminate a VLC process:\n{killEx.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error attempting to terminate VLC:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private async void restartVLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // First: Kill VLC like exit
                var vlcProcesses = System.Diagnostics.Process.GetProcessesByName("vlc");
                foreach (var process in vlcProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(2000); // Wait up to 2 seconds for exit
                    }
                    catch (Exception killEx)
                    {
                        MessageBox.Show(
                            $"Failed to terminate a VLC process:\n{killEx.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }

                // Wait 5 seconds before restart
                await Task.Delay(5000);

                // Find VLC in standard locations
                string vlcPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
                if (!System.IO.File.Exists(vlcPath))
                    vlcPath = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";

                if (!System.IO.File.Exists(vlcPath))
                {
                    MessageBox.Show(
                        "VLC media player was not found in the standard installation locations.\n" +
                        "Please install VLC or configure its path.",
                        "VLC Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // Relaunch VLC
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = vlcPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error attempting to restart VLC:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ================================
        // Scraping Logic - Buttons
        // ================================
        private async void btnScrapeMD1_Click(object sender, EventArgs e)
        {
            await StartScrapeForDrive("Media Drive 1");
        }
        private async void btnScrapeMD2_Click(object sender, EventArgs e)
        {
            await StartScrapeForDrive("Media Drive 2");
        }
        private async void btnScrapeMD3_Click(object sender, EventArgs e)
        {
            await StartScrapeForDrive("Media Drive 3");
        }
        private async void btnScrapeMD4_Click(object sender, EventArgs e)
        {
            await StartScrapeForDrive("Media Drive 4");
        }
        private async void btnScrapeMD5_Click(object sender, EventArgs e)
        {
            await StartScrapeForDrive("Media Drive 5");
        }
        private async void btnScrapeMD6_Click(object sender, EventArgs e)
        {
            await StartScrapeForDrive("Media Drive 6");
        }
        private async void btnScrapeAuto_Click(object sender, EventArgs e)
        {
            await AutoScrapeAllDrivesAsync();
        }
        private async Task AutoScrapeAllDrivesAsync()
        {
            // List of all drive names to process in order
            var driveNames = new[]
            {
        "Media Drive 1",
        "Media Drive 2",
        "Media Drive 3",
        "Media Drive 4",
        "Media Drive 5",
        "Media Drive 6"
    };

            // Disable all scrape buttons (including Auto button)
            SetScrapeButtonsEnabled(false);

            try
            {
                for (int i = 0; i < driveNames.Length; i++)
                {
                    string drive = driveNames[i];

                    // Optional: Update UI to show which drive is currently being scanned
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            labScraperLocal.Text = $"Auto-scanning: {drive} ({i + 1} of {driveNames.Length})...";
                            progressBarScraperLocal.Value = 0;
                        }));
                    }
                    else
                    {
                        labScraperLocal.Text = $"Auto-scanning: {drive} ({i + 1} of {driveNames.Length})...";
                        progressBarScraperLocal.Value = 0;
                    }

                    try
                    {
                        // Await the scan for the current drive, do NOT re-enable buttons here!
                        await StartScrapeForDrive(drive, reenableButtons: false);
                    }
                    catch (Exception ex)
                    {
                        // Report error but continue to next drive
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                labScraperLocal.Text = $"Error scanning {drive}: {ex.Message}";
                            }));
                        }
                        else
                        {
                            labScraperLocal.Text = $"Error scanning {drive}: {ex.Message}";
                        }
                        await Task.Delay(1000); // Small pause before next drive
                    }
                }

                // All drives done
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        labScraperLocal.Text = "Auto scan complete!";
                        progressBarScraperLocal.Value = 100;
                    }));
                }
                else
                {
                    labScraperLocal.Text = "Auto scan complete!";
                    progressBarScraperLocal.Value = 100;
                }
            }
            finally
            {
                // Re-enable buttons at the end, regardless of errors
                SetScrapeButtonsEnabled(true);
            }
        }
        private async Task StartScrapeForDrive(string driveName, bool reenableButtons = true)
        {
            // Optionally disable all scraper buttons to prevent overlap
            SetScrapeButtonsEnabled(false);
            progressBarScraperLocal.Value = 0;
            labScraperLocal.Text = $"Starting scrape for {driveName}...";

            try
            {
                var config = new ScraperConfig();
                var scraper = new ScraperController(config, _notionManager); // _notionManager assumed initialized

                await Task.Run(() => scraper.RunScrapeAsync(driveName, job =>
                {
                    int percent = (job.TotalFiles > 0) ? (int)((float)job.FilesProcessed / job.TotalFiles * 100) : 0;
                    percent = Math.Clamp(percent, 0, 100);

                    // Ensure UI updates occur on the UI thread
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            progressBarScraperLocal.Value = percent;
                            labScraperLocal.Text = job.StatusMessage;
                        }));
                    }
                    else
                    {
                        progressBarScraperLocal.Value = percent;
                        labScraperLocal.Text = job.StatusMessage;
                    }
                }));
                labScraperLocal.Text = $"Scrape for {driveName} complete!";
            }
            catch (Exception ex)
            {
                labScraperLocal.Text = $"Error: {ex.Message}";
            }
            finally
            {
                // Re-enable buttons only if the caller requested it (single-drive mode)
                if (reenableButtons)
                    SetScrapeButtonsEnabled(true);
            }
        }
        private void SetScrapeButtonsEnabled(bool enabled)
        {
            btnScrapeMD1.Enabled = enabled;
            btnScrapeMD2.Enabled = enabled;
            btnScrapeMD3.Enabled = enabled;
            btnScrapeMD4.Enabled = enabled;
            btnScrapeMD5.Enabled = enabled;
            btnScrapeMD6.Enabled = enabled;
            btnScrapeAuto.Enabled = enabled;
        }
        // ================================




        private async void btnSetStart_Click(object sender, EventArgs e)
        {
            // 1. Get the latest playback state and the current media item
            var playbackState = _vlcPlaybackController?.GetCurrentState();
            var currentItem = playbackState?.CurrentMedia;

            // 2. Validate current item and Notion data
            if (currentItem == null || !currentItem.NotionLoaded || currentItem.NotionData == null)
            {
                Logger.Info("[btnSetStart] No media is currently loaded or Notion data is missing.");
                return;
            }

            // 3. Get Notion Page ID
            string pageId = currentItem.NotionData.PageId;
            if (string.IsNullOrEmpty(pageId))
            {
                Logger.Info("[btnSetStart] Could not find Notion Page ID for this media.");
                return;
            }

            // 4. Capture the most up-to-date current playback time (in seconds)
            int currentTimeSeconds = (int)playbackState.Position.TotalSeconds;

            // Workaround: If time is zero but UI says otherwise, parse from label
            if (currentTimeSeconds <= 1 && labCurrentTime != null && !string.IsNullOrWhiteSpace(labCurrentTime.Text))
            {
                if (TimeSpan.TryParse(labCurrentTime.Text, out var labelTime))
                {
                    int labelSeconds = (int)labelTime.TotalSeconds;
                    // Only use if label is "ahead" of playbackState
                    if (labelSeconds > 1)
                    {
                        Logger.Info($"[btnSetStart] PlaybackState.Position was {currentTimeSeconds} but labCurrentTime.Text parsed as {labelSeconds}; using label value.");
                        currentTimeSeconds = labelSeconds;
                    }
                }
            }

            // 5. Prepare tags for update
            var tags = currentItem.NotionData.Tags?.ToList() ?? new List<string>();

            // Remove only the Start-Set and Start-Unset tags
            tags.RemoveAll(t => t.Equals("Start-Set", StringComparison.OrdinalIgnoreCase) ||
                                t.Equals("Start-Unset", StringComparison.OrdinalIgnoreCase));

            // Add "Start-Set" tag (preserving all other tags)
            tags.Add("Start-Set");

            // 6. Prepare Notion property keys
            string startTimeKey = ConfigManager.GetAppSettingValue("StartTime_Notion");
            string tagsKey = ConfigManager.GetAppSettingValue("Tags_Notion");

            // 7. Build property updates dictionary
            var updates = new Dictionary<string, object>
    {
        { startTimeKey, currentTimeSeconds },
        { tagsKey, tags }
    };

            // 8. Send update to Notion and update UI only on success
            try
            {
                Logger.Info($"[btnSetStart] Attempting to update Notion page '{pageId}' with StartTime={currentTimeSeconds} and updated tags...");

                await _notionManager.UpdateNotionPagePropertyAsync(pageId, updates);

                // Assume success on no exception
                currentItem.StartTimeSeconds = currentTimeSeconds;
                currentItem.NotionData.Tags = tags;
                labStartMediaTag.Text = "Start-Set";

                Logger.Info("[btnSetStart] Successfully updated Notion and UI label set to 'Start-Set'.");
            }
            catch (Exception ex)
            {
                Logger.Error("[btnSetStart] Failed to update Notion.", ex);
                // Do NOT update label on failure
            }
        }


        private async void btnSetEnd_Click(object sender, EventArgs e)
        {
            // 1. Get the latest playback state and the current media item
            var playbackState = _vlcPlaybackController?.GetCurrentState();
            var currentItem = playbackState?.CurrentMedia;

            // 2. Validate current item and Notion data
            if (currentItem == null || !currentItem.NotionLoaded || currentItem.NotionData == null)
            {
                Logger.Info("[btnSetEnd] No media is currently loaded or Notion data is missing.");
                return;
            }

            // 3. Get Notion Page ID
            string pageId = currentItem.NotionData.PageId;
            if (string.IsNullOrEmpty(pageId))
            {
                Logger.Info("[btnSetEnd] Could not find Notion Page ID for this media.");
                return;
            }

            // 4. Capture the most up-to-date current playback time (in seconds)
            int currentTimeSeconds = (int)playbackState.Position.TotalSeconds;

            // Workaround: If time is zero but UI says otherwise, parse from label
            if (currentTimeSeconds <= 1 && labCurrentTime != null && !string.IsNullOrWhiteSpace(labCurrentTime.Text))
            {
                if (TimeSpan.TryParse(labCurrentTime.Text, out var labelTime))
                {
                    int labelSeconds = (int)labelTime.TotalSeconds;
                    if (labelSeconds > 1)
                    {
                        Logger.Info($"[btnSetEnd] PlaybackState.Position was {currentTimeSeconds} but labCurrentTime.Text parsed as {labelSeconds}; using label value.");
                        currentTimeSeconds = labelSeconds;
                    }
                }
            }

            // 5. Prepare tags for update
            var tags = currentItem.NotionData.Tags?.ToList() ?? new List<string>();

            // Remove only the End-Set and End-Unset tags
            tags.RemoveAll(t => t.Equals("End-Set", StringComparison.OrdinalIgnoreCase) ||
                                t.Equals("End-Unset", StringComparison.OrdinalIgnoreCase));

            // Add "End-Set" tag (preserving all other tags)
            tags.Add("End-Set");

            // 6. Prepare Notion property keys
            string endTimeKey = ConfigManager.GetAppSettingValue("EndTime_Notion");
            string tagsKey = ConfigManager.GetAppSettingValue("Tags_Notion");

            // 7. Build property updates dictionary
            var updates = new Dictionary<string, object>
    {
        { endTimeKey, currentTimeSeconds },
        { tagsKey, tags }
    };

            // 8. Send update to Notion and update UI only on success
            try
            {
                Logger.Info($"[btnSetEnd] Attempting to update Notion page '{pageId}' with EndTime={currentTimeSeconds} and updated tags...");

                await _notionManager.UpdateNotionPagePropertyAsync(pageId, updates);

                // Assume success on no exception
                currentItem.EndTimeSeconds = currentTimeSeconds;
                currentItem.NotionData.Tags = tags;
                labEndMediaTag.Text = "End-Set";

                Logger.Info("[btnSetEnd] Successfully updated Notion and UI label set to 'End-Set'.");
            }
            catch (Exception ex)
            {
                Logger.Error("[btnSetEnd] Failed to update Notion.", ex);
                // Do NOT update label on failure
            }
        }


        private async void btnSetVol_Click(object sender, EventArgs e)
        {
            // 1. Get the latest playback state and the current media item
            var playbackState = _vlcPlaybackController?.GetCurrentState();
            var currentItem = playbackState?.CurrentMedia;

            // 2. Validate current item and Notion data
            if (currentItem == null || !currentItem.NotionLoaded || currentItem.NotionData == null)
            {
                Logger.Warning("[btnSetVol] No media is currently loaded or Notion data is missing.");
                return;
            }

            // 3. Get Notion Page ID
            string pageId = currentItem.NotionData.PageId;
            if (string.IsNullOrEmpty(pageId))
            {
                Logger.Warning("[btnSetVol] Could not find Notion Page ID for this media.");
                return;
            }

            // 4. Capture the most up-to-date current playback volume (percentage)
            int currentVolume = playbackState.Volume;

            // Workaround: If volume is zero but UI says otherwise, parse from UI element
            if ((currentVolume <= 1) && trackBarVolume != null)
            {
                currentVolume = trackBarVolume.Value;
                Logger.Debug($"[btnSetVol] PlaybackState.Volume was 0, using trackBarVolume.Value={currentVolume}.");
            }

            // 5. Prepare tags for update
            var tags = currentItem.NotionData.Tags?.ToList() ?? new List<string>();

            // Remove only the Vol-Set and Vol-Unset tags
            tags.RemoveAll(t => t.Equals("Vol-Set", StringComparison.OrdinalIgnoreCase) ||
                                t.Equals("Vol-Unset", StringComparison.OrdinalIgnoreCase));

            // Add "Vol-Set" tag (preserving all other tags)
            tags.Add("Vol-Set");

            // 6. Prepare Notion property keys
            string volumeKey = ConfigManager.GetAppSettingValue("Volume_Notion");
            string tagsKey = ConfigManager.GetAppSettingValue("Tags_Notion");

            // 7. Build property updates dictionary
            var updates = new Dictionary<string, object>
    {
        { volumeKey, currentVolume },
        { tagsKey, tags }
    };

            // 8. Send update to Notion and update UI only on success
            try
            {
                Logger.Info($"[btnSetVol] Attempting to update Notion page '{pageId}' with Volume={currentVolume} and updated tags...");

                await _notionManager.UpdateNotionPagePropertyAsync(pageId, updates);

                // Assume success on no exception
                currentItem.NotionData.Volume = currentVolume;
                currentItem.NotionData.Tags = tags;
                labVolMediaTag.Text = "Vol-Set";

                Logger.Info("[btnSetVol] Successfully updated Notion and UI label set to 'Vol-Set'.");
            }
            catch (Exception ex)
            {
                Logger.Error("[btnSetVol] Failed to update Notion.", ex);
                // Do NOT update label on failure
            }
        }

        private void SetTagButtonsEnabled(bool enabled)
        {
            btnSetStart.Enabled = enabled;
            btnSetEnd.Enabled = enabled;
            btnSetVol.Enabled = enabled;
        }
        /// <summary>
        /// Updates the Start, End, and Vol tag labels to reflect the tags of the current media.
        /// </summary>
        private void UpdateTagLabelsFromCurrentMedia(MediaItem currentMedia)
        {
            // If media or Notion data is null, tags are still loading
            if (currentMedia?.NotionData == null)
            {
                labStartMediaTag.Text = "Loading...";
                labEndMediaTag.Text = "Loading...";
                labVolMediaTag.Text = "Loading...";
                return;
            }

            // If Notion tags are not loaded yet, still loading
            var tags = currentMedia.NotionData.Tags;
            if (tags == null)
            {
                labStartMediaTag.Text = "Loading...";
                labEndMediaTag.Text = "Loading...";
                labVolMediaTag.Text = "Loading...";
                return;
            }

            // Show "Set" or "Unset" based on tag presence
            labStartMediaTag.Text = tags.Any(t => t.Equals("Start-Set", StringComparison.OrdinalIgnoreCase))
                ? "Start-Set"
                : "Start-Unset";

            labEndMediaTag.Text = tags.Any(t => t.Equals("End-Set", StringComparison.OrdinalIgnoreCase))
                ? "End-Set"
                : "End-Unset";

            labVolMediaTag.Text = tags.Any(t => t.Equals("Vol-Set", StringComparison.OrdinalIgnoreCase))
                ? "Vol-Set"
                : "Vol-Unset";
        }


    }
}