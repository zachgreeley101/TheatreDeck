using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using theatredeck.app.core.api.notion;
using theatredeck.app.core.api.notion.models;
using theatredeck.app.core.logger;
using theatredeck.app.core.utils;
using theatredeck.app.features.vlc.interfaces;
using theatredeck.app.features.vlc.models;
using theatredeck.app.features.vlc.utils;

namespace theatredeck.app.features.vlc.controllers
{
    /// <summary>
    /// Coordinates playlist management and high-level VLC playback control.
    /// </summary>
    public class PlaybackController : IServiceVLC, IDisposable
    {
        private HttpService _httpService;
        private NotionManager _notionManager;
        private readonly List<MediaItem> _playlist = new List<MediaItem>();
        private int _currentIndex = -1;
        private bool _isMuted = false;
        private CancellationTokenSource _pollingCts;
        private readonly List<IEventSubscriberVLC> _subscribers = new List<IEventSubscriberVLC>();
        private PlaybackState _currentPlaybackState = new PlaybackState();
        public int CurrentPlaylistIndex => _currentIndex;
        private bool _vlcConnected = false;
        // --- Auto-refresh for 5-minute time updates ---
        private int _fiveMinutePollCounter = 0;
        private string _lastPolledMediaFilePath = null;
        // Five minutes worth of polling cycles (1 poll/sec = 300 cycles)
        private const int FiveMinutePollThreshold = 300;

        public PlaybackController(NotionManager notionManager)
        {
            _httpService = new HttpService();
            _notionManager = notionManager ?? throw new ArgumentNullException(nameof(notionManager));
            Logger.Info("[PlaybackController] Initialized.");
        }



        public async Task Play()
        {
            try
            {
                Logger.Info("[PlaybackController] Play requested.");

                if (_currentIndex < 0 && _playlist.Count > 0)
                    _currentIndex = 0;

                if (_currentIndex >= 0 && _currentIndex < _playlist.Count)
                {
                    var item = _playlist[_currentIndex];
                    var file = item?.FilePath ?? "(null)";

                    Logger.Info($"[PlaybackController] Playing media at index {_currentIndex}: {file}");

                    await _httpService.PlayMediaAsync(file);

                    // ===== Now/Next Status Integration with Looping =====
                    MediaItem nextItem = null;
                    if (_playlist.Count == 1)
                    {
                        nextItem = item;
                    }
                    else if (_playlist.Count > 1)
                    {
                        int nextIndex = (_currentIndex + 1) % _playlist.Count;
                        nextItem = _playlist[nextIndex];
                    }

                    // --- IMMEDIATE UI/Label Update ---
                    NowNextStatusWriter.WriteNowAndNext(item, nextItem);
                    NotifyPlaybackStateChanged(EventType.Playing);

                    // Wait for VLC to start playing before seeking (if needed)
                    if (item != null && item.StartTimeSeconds > 0)
                    {
                        Logger.Debug("[PlaybackController] Waiting for VLC to reach 'playing' state before seek...");
                        await WaitForVlcToStartPlaying();
                        Logger.Info($"[PlaybackController] Seeking to StartTimeSeconds: {item.StartTimeSeconds}");
                        await _httpService.SeekAsync(item.StartTimeSeconds);
                    }

                    // Determine target volume (Notion or default)
                    int targetVolume = 100;
                    if (item != null && item.NotionLoaded && item.NotionData != null && item.NotionData.Volume.HasValue)
                    {
                        targetVolume = item.NotionData.Volume.Value;
                        Logger.Info($"[PlaybackController] Setting volume from Notion to {targetVolume}%.");
                    }
                    else
                    {
                        Logger.Warning("[PlaybackController] No Notion volume found, defaulting to 100%.");
                    }

                    // Gradually fade to the target volume over 7 seconds (run in background)
                    int lastVolume = _currentPlaybackState?.Volume ?? 100;
                    _ = FadeVolumeAsync(lastVolume, targetVolume); // Fire and forget (non-blocking)
                }
                else
                {
                    Logger.Info("[PlaybackController] Resuming playback (no index).");
                    await _httpService.PlayAsync();

                    // Now/Next as in original
                    MediaItem current = (_currentIndex >= 0 && _currentIndex < _playlist.Count) ? _playlist[_currentIndex] : null;
                    MediaItem next = null;
                    if (_playlist.Count == 1 && current != null)
                    {
                        next = current;
                    }
                    else if (_playlist.Count > 1 && _currentIndex >= 0)
                    {
                        int nextIndex = (_currentIndex + 1) % _playlist.Count;
                        next = _playlist[nextIndex];
                    }

                    // --- IMMEDIATE UI/Label Update ---
                    NowNextStatusWriter.WriteNowAndNext(current, next);
                    NotifyPlaybackStateChanged(EventType.Playing);

                    // --- Update time files to reflect resume ---
                    int startSeconds = current?.StartTimeSeconds ?? 0;
                    int endSeconds = current?.EndTimeSeconds ?? (current?.Duration?.TotalSeconds is double d ? (int)d : 0);
                    int segmentDuration = endSeconds > startSeconds ? endSeconds - startSeconds : 0;

                    // === NEW: Write actual current time, not zero, when resuming ===
                    double currentPosition = _currentPlaybackState?.Position.TotalSeconds ?? 0;
                    int relativePosition = Math.Max(0, (int)currentPosition - startSeconds);
                    NowNextStatusWriter.WritePlaybackTimes(relativePosition, segmentDuration);

                    // Fade to default (100%) on resume (run in background)
                    int lastVolume = _currentPlaybackState?.Volume ?? 100;
                    _ = FadeVolumeAsync(lastVolume, 100);
                    Logger.Info("[PlaybackController] Resume: Faded volume to 100% by default.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[PlaybackController] Error in Play()", ex);
            }
        }
        public async Task PlayMediaAtIndex(int index)
        {
            if (index < 0 || index >= _playlist.Count)
            {
                Logger.Warning($"[PlaybackController] PlayMediaAtIndex aborted: invalid index {index} (playlistCount={_playlist.Count})");
                return;
            }

            try
            {
                _currentIndex = index;
                var item = _playlist[_currentIndex];
                var file = item?.FilePath ?? "(null)";

                Logger.Info($"[PlaybackController] Playing media at index {index}: '{item?.DisplayName ?? "null"}', filePath='{file}'");

                await _httpService.PlayMediaAsync(file);

                // Now/Next status integration with looping
                MediaItem nextItem = null;
                if (_playlist.Count == 1)
                {
                    nextItem = item;
                }
                else if (_playlist.Count > 1)
                {
                    int nextIndex = (_currentIndex + 1) % _playlist.Count;
                    nextItem = _playlist[nextIndex];
                }

                // --- IMMEDIATE UI/Label Update ---
                NowNextStatusWriter.WriteNowAndNext(item, nextItem);

                NotifyPlaybackStateChanged(EventType.Playing);

                // Wait for VLC to start playing before seeking (if needed)
                if (item != null && item.StartTimeSeconds > 0)
                {
                    await WaitForVlcToStartPlaying();
                    Logger.Info($"[PlaybackController] Seeking to StartTimeSeconds={item.StartTimeSeconds} for '{item.DisplayName}'");
                    await _httpService.SeekAsync(item.StartTimeSeconds);
                }

                // Set volume from Notion if available, else default to 100% (with fade), AFTER seek!
                int targetVolume = 100; // Default to 100%
                if (item != null && item.NotionLoaded && item.NotionData != null && item.NotionData.Volume.HasValue)
                {
                    targetVolume = item.NotionData.Volume.Value;
                    Logger.Info($"[PlaybackController] Setting volume from Notion to {targetVolume}%.");
                }
                else
                {
                    Logger.Warning("[PlaybackController] No Notion volume found, defaulting to 100%.");
                }

                int lastVolume = _currentPlaybackState?.Volume ?? 100;
                _ = FadeVolumeAsync(lastVolume, targetVolume); // Fire and forget (non-blocking)
            }
            catch (Exception ex)
            {
                Logger.Error($"[PlaybackController] Error in PlayMediaAtIndex({index})", ex);
            }
        }
        private async Task WaitForVlcToStartPlaying(int maxAttempts = 20, int delayMs = 100)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    string statusXml = await _httpService.GetStatusXmlAsync();
                    var stateStr = ParseVlcStateFromXml(statusXml);
                    if (stateStr == "playing")
                        return;
                }
                catch
                {
                    // Ignore and try again
                }
                await Task.Delay(delayMs);
            }

            Logger.Warning("[PlaybackController] WaitForVlcToStartPlaying timed out.");
        }
        private string ParseVlcStateFromXml(string xml)
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                return doc.Root.Element("state")?.Value ?? "";
            }
            catch (Exception ex)
            {
                Logger.Warning("[PlaybackController] Failed to parse VLC state from XML.", ex);
                return "";
            }
        }
        public async Task Pause()
        {
            try
            {
                // Get the current VLC state (fetch status XML)
                string statusXml = await _httpService.GetStatusXmlAsync();
                string stateStr = ParseVlcStateFromXml(statusXml); // Helper already in your code

                if (stateStr == "paused")
                {
                    // VLC is currently paused, so pressing button will RESUME

                    // Write current playback time to data files
                    MediaItem current = (_currentIndex >= 0 && _currentIndex < _playlist.Count) ? _playlist[_currentIndex] : null;
                    int startSeconds = current?.StartTimeSeconds ?? 0;
                    int endSeconds = current?.EndTimeSeconds ?? (current?.Duration?.TotalSeconds is double d ? (int)d : 0);
                    int segmentDuration = endSeconds > startSeconds ? endSeconds - startSeconds : 0;

                    // Fetch current position from VLC status
                    int positionSeconds = 0;
                    int.TryParse(GetXmlElementValue(statusXml, "time"), out positionSeconds);
                    int relativePosition = Math.Max(0, positionSeconds - startSeconds);

                    NowNextStatusWriter.WritePlaybackTimes(relativePosition, segmentDuration);

                    // Resume playback
                    await _httpService.PauseAsync(); // Toggles from paused to playing
                    NotifyPlaybackStateChanged(EventType.Playing);

                    Logger.Info("[PlaybackController] Pause called while already paused. Resumed playback and wrote current time to files.");
                }
                else
                {
                    // VLC is currently playing, so pressing button will PAUSE
                    await _httpService.PauseAsync();
                    NotifyPlaybackStateChanged(EventType.Paused);

                    // Write "pause" to time files
                    NowNextStatusWriter.WritePauseToTimeFiles();

                    Logger.Info("[PlaybackController] Pause called while playing. Wrote 'pause' to files.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[PlaybackController] Error in Pause()", ex);
            }
        }
        private string GetXmlElementValue(string xml, string elementName)
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                return doc.Root.Element(elementName)?.Value ?? "";
            }
            catch (Exception ex)
            {
                Logger.Warning($"[PlaybackController] Failed to get XML element '{elementName}' value.", ex);
                return "";
            }
        }
        public async Task Stop()
        {
            try
            {
                Logger.Info("[PlaybackController] Stop requested.");
                await _httpService.StopAsync();
                NotifyPlaybackStateChanged(EventType.Stopped);

                // ===== Now/Next Status Integration =====
                NowNextStatusWriter.ClearStatus();

                // === Write "stop" to time files ===
                NowNextStatusWriter.WriteStopToTimeFiles();
            }
            catch (Exception ex)
            {
                Logger.Error("[PlaybackController] Error in Stop()", ex);
            }
        }
        public async Task AddMedia(MediaItem item)
        {
            if (item != null)
            {
                // Prevent duplicates based on file path (case-insensitive)
                if (_playlist.Any(m => string.Equals(m.FilePath, item.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.Warning($"[PlaybackController] Duplicate media not added: {item.FilePath}");
                    return;
                }

                bool wasEmpty = _playlist.Count == 0;
                _playlist.Add(item);
                Logger.Info($"[PlaybackController] Media added: {item.FilePath}");

                // If playlist was empty, set this as the current index (auto-cue first item)
                if (wasEmpty)
                    _currentIndex = 0;

                NotifyPlaylistChanged();

                // --- Update Now/Next Status after modification ---
                if (_playlist.Count > 0 && _currentIndex >= 0 && _currentIndex < _playlist.Count)
                {
                    var current = _playlist[_currentIndex];
                    MediaItem next = null;
                    if (_playlist.Count == 1)
                        next = current;
                    else
                        next = _playlist[(_currentIndex + 1) % _playlist.Count];

                    NowNextStatusWriter.WriteNowAndNext(current, next);
                }
            }
        }
        public async Task RemoveMedia(MediaItem item)
        {
            if (item == null)
            {
                Logger.Warning("[PlaybackController] RemoveMedia called with null item. Aborting.");
                return;
            }

            if (!_playlist.Contains(item))
            {
                Logger.Warning($"[PlaybackController] RemoveMedia: item '{item.DisplayName}' not found in playlist. Aborting.");
                return;
            }

            int removedIndex = _playlist.IndexOf(item);
            Logger.Info($"[PlaybackController] RemoveMedia called. Removing '{item.DisplayName}' at index {removedIndex}. " +
                        $"CurrentIndex={_currentIndex}, PlaylistCount={_playlist.Count}");

            // Show the playlist order before removal
            Logger.Debug($"[PlaybackController] [BEFORE] Playlist order: {string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"))}");

            _playlist.RemoveAt(removedIndex);

            // Log the new playlist order immediately after removal
            Logger.Debug($"[PlaybackController] [AFTER] Playlist order: {string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"))}");

            // Adjust current index if needed
            if (_playlist.Count == 0)
            {
                Logger.Info("[PlaybackController] Playlist now empty after removal. Resetting currentIndex and calling Stop().");
                _currentIndex = -1;
                Stop(); // Stop playback if playlist is now empty
            }
            else if (_currentIndex == removedIndex)
            {
                Logger.Info("[PlaybackController] Removed item was currently playing. Adjusting index and advancing playback.");
                // If we removed the currently playing item, play the next if possible
                if (_currentIndex >= _playlist.Count) // Was last item
                {
                    Logger.Info("[PlaybackController] Removed last item; currentIndex out of bounds, correcting.");
                    _currentIndex = _playlist.Count - 1;
                }
                await PlayMediaAtIndex(_currentIndex);
            }
            else if (removedIndex < _currentIndex)
            {
                _currentIndex--;
                Logger.Debug($"[PlaybackController] Removed item was before the currentIndex. Decrementing currentIndex to {_currentIndex}.");
            }
            else
            {
                Logger.Debug($"[PlaybackController] Removed item did not affect currentIndex. currentIndex remains {_currentIndex}.");
            }

            NotifyPlaylistChanged();
        }
        public async Task ClearPlaylist()
        {
            _playlist.Clear();
            _currentIndex = -1;
            Logger.Info("[PlaybackController] Playlist cleared.");
            NotifyPlaylistChanged();
            await Task.CompletedTask; // Not necessary, but legal
        }
        public async Task<int> MoveMediaUp(int index)
        {
            Logger.Info($"[PlaybackController] MoveMediaUp called. index={index}, currentIndex={_currentIndex}, playlistCount={_playlist.Count}");

            // Edge: Empty playlist or invalid index
            if (_playlist.Count == 0)
            {
                Logger.Warning("[PlaybackController] MoveMediaUp aborted: playlist is empty.");
                return index;
            }
            if (index <= 0 || index >= _playlist.Count)
            {
                Logger.Warning($"[PlaybackController] MoveMediaUp aborted: invalid index {index} (playlistCount={_playlist.Count})");
                return index;
            }

            Logger.Debug($"[PlaybackController] [BEFORE] Playlist order: {string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"))}");

            var movingUpItem = _playlist[index];
            var aboveItem = _playlist[index - 1];
            Logger.Debug($"[PlaybackController] Swapping: [{index}] '{movingUpItem.DisplayName}' with [{index - 1}] '{aboveItem.DisplayName}'");

            // Swap items
            _playlist[index - 1] = movingUpItem;
            _playlist[index] = aboveItem;

            // Adjust current index if affected
            if (_currentIndex == index)
            {
                _currentIndex--;
                Logger.Debug($"[PlaybackController] currentIndex matched moved item. New currentIndex={_currentIndex}");
            }
            else if (_currentIndex == index - 1)
            {
                _currentIndex++;
                Logger.Debug($"[PlaybackController] currentIndex matched above item. New currentIndex={_currentIndex}");
            }
            else
            {
                Logger.Debug($"[PlaybackController] currentIndex unaffected: {_currentIndex}");
            }

            Logger.Debug($"[PlaybackController] [AFTER] Playlist order: {string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"))}");
            Logger.Info($"[PlaybackController] [AFTER] MoveUp complete. currentIndex={_currentIndex}, now-playing={_playlist.ElementAtOrDefault(_currentIndex)?.DisplayName ?? "None"}");

            NotifyPlaylistChanged();

            // Return the new index of the moved item so the UI can restore selection
            return index - 1;
        }
        private void LogPlaylistOrder()
        {
            var order = string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"));
            Logger.Debug($"[PlaybackController] [PLAYLIST ORDER] {order}");
        }
        public async Task<int> MoveMediaDown(int index)
        {
            Logger.Info($"[PlaybackController] MoveMediaDown called. index={index}, currentIndex={_currentIndex}, playlistCount={_playlist.Count}");

            // Edge: Empty playlist or invalid index
            if (_playlist.Count == 0)
            {
                Logger.Warning("[PlaybackController] MoveMediaDown aborted: playlist is empty.");
                return index;
            }
            if (index < 0 || index >= _playlist.Count - 1)
            {
                Logger.Warning($"[PlaybackController] MoveMediaDown aborted: invalid index {index} (playlistCount={_playlist.Count})");
                return index;
            }

            Logger.Debug($"[PlaybackController] [BEFORE] Playlist order: {string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"))}");

            var movingDownItem = _playlist[index];
            var belowItem = _playlist[index + 1];
            Logger.Debug($"[PlaybackController] Swapping: [{index}] '{movingDownItem.DisplayName}' with [{index + 1}] '{belowItem.DisplayName}'");

            // Swap items
            _playlist[index] = belowItem;
            _playlist[index + 1] = movingDownItem;

            // Adjust current index if affected
            if (_currentIndex == index)
            {
                _currentIndex++;
                Logger.Debug($"[PlaybackController] currentIndex matched moved item. New currentIndex={_currentIndex}");
            }
            else if (_currentIndex == index + 1)
            {
                _currentIndex--;
                Logger.Debug($"[PlaybackController] currentIndex matched below item. New currentIndex={_currentIndex}");
            }
            else
            {
                Logger.Debug($"[PlaybackController] currentIndex unaffected: {_currentIndex}");
            }

            Logger.Debug($"[PlaybackController] [AFTER] Playlist order: {string.Join(" | ", _playlist.Select((m, i) => $"{i}:{m.DisplayName}"))}");
            Logger.Info($"[PlaybackController] [AFTER] MoveDown complete. currentIndex={_currentIndex}, now-playing={_playlist.ElementAtOrDefault(_currentIndex)?.DisplayName ?? "None"}");

            NotifyPlaylistChanged();

            // Return the new index of the moved item so the UI can restore selection
            return index + 1;
        }
        public void Subscribe(IEventSubscriberVLC subscriber)
        {
            if (subscriber != null && !_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                Logger.Info($"[PlaybackController] Subscriber added: {subscriber.GetType().Name}");
            }
        }
        public void Unsubscribe(IEventSubscriberVLC subscriber)
        {
            if (subscriber != null && _subscribers.Contains(subscriber))
            {
                _subscribers.Remove(subscriber);
                Logger.Info($"[PlaybackController] Subscriber removed: {subscriber.GetType().Name}");
            }
        }
        public PlaybackState GetCurrentState()
        {
            return new PlaybackState
            {
                CurrentMedia = (_currentIndex >= 0 && _currentIndex < _playlist.Count)
                    ? _playlist[_currentIndex]
                    : null,
                Position = TimeSpan.Zero, // Extend with polling/HTTP status if desired
                Duration = null,
                State = EventType.None,
                IsMuted = _isMuted,
                ErrorMessage = null
            };
        }
        public IList<MediaItem> GetPlaylist() => _playlist.ToList();
        public async Task Seek(TimeSpan position)
        {
            // Implement seek via HTTP if desired
            Logger.Warning("[PlaybackController] Seek not yet implemented.");
        }
        private void NotifyPlaybackStateChanged(EventType evt)
        {
            PlaybackState state;

            if (evt == EventType.Searching)
            {
                // Build a "searching" playback state
                state = new PlaybackState
                {
                    State = EventType.Searching,
                    CurrentMedia = null,
                    Position = TimeSpan.Zero,
                    Duration = null,
                    IsMuted = false,
                    ErrorMessage = null,
                    Volume = 0
                };
                _currentPlaybackState = state; // Optionally, keep this as the latest state
            }
            else
            {
                // Use the actual, updated playback state
                _currentPlaybackState.State = evt;
                state = _currentPlaybackState;
            }

            // Optional: Log only actual state change, not every notification
            // Logger.Info($"[PlaybackController] State changed: {evt}");

            foreach (var sub in _subscribers)
            {
                sub.OnPlaybackStateChanged(state);
            }
            foreach (var sub in _subscribers)
            {
                sub.OnPlaybackEvent(evt, state);
            }
        }
        private void NotifyPlaylistChanged()
        {
            Logger.Info("[PlaybackController] Playlist changed (event placeholder).");

            // Log the full playlist order
            LogPlaylistOrder();

            // Log the currently playing index and file (if any)
            if (_currentIndex >= 0 && _currentIndex < _playlist.Count)
            {
                Logger.Info($"[PlaybackController] Now playing index: {_currentIndex}, file: {_playlist[_currentIndex].DisplayName}");
            }
            else
            {
                Logger.Info("[PlaybackController] No item is currently playing.");
            }
        }
        public void Dispose()
        {
            _httpService?.Dispose();
            Logger.Info("[PlaybackController] Disposed.");
        }
        public void StartPollingVlcStatus()
        {
            Logger.Info("[PlaybackController] StartPollingVlcStatus called.");

            // Prevent multiple pollers
            if (_pollingCts != null && !_pollingCts.IsCancellationRequested)
            {
                Logger.Warning("[PlaybackController] Polling already active. Skipping new start.");
                return;
            }

            _pollingCts = new CancellationTokenSource();
            Logger.Info("[PlaybackController] Starting PollVlcStatusAsync background task.");
            Task.Run(() => PollVlcStatusAsync(_pollingCts.Token));
        }
        public void StopPollingVlcStatus()
        {
            _pollingCts?.Cancel();
        }
        private async Task PollVlcStatusAsync(CancellationToken token)
        {
            if (!_vlcConnected)
                SetSearchingState();

            double lastKnownVlcPosition = -1;
            string lastMediaFilePath = null;

            // --- Begin polling loop ---
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string statusXml = await _httpService.GetStatusXmlAsync();
                    UpdatePlaybackStateFromXml(statusXml);

                    if (_currentIndex >= 0 && _currentIndex < _playlist.Count)
                    {
                        var currentItem = _playlist[_currentIndex];
                        var currentPosition = _currentPlaybackState.Position.TotalSeconds;

                        int startSeconds = currentItem?.StartTimeSeconds ?? 0;
                        int endSeconds = currentItem?.EndTimeSeconds ?? (currentItem?.Duration?.TotalSeconds is double d ? (int)d : 0);
                        int segmentDuration = endSeconds > startSeconds ? endSeconds - startSeconds : 0;
                        int relativePosition = Math.Max(0, (int)currentPosition - startSeconds);

                        // --- Only update timer files if actually playing ---
                        string currentMediaFilePath = currentItem?.FilePath;
                        if (currentMediaFilePath != null && currentMediaFilePath != lastMediaFilePath)
                        {
                            if (_currentPlaybackState?.State == EventType.Playing)
                            {
                                NowNextStatusWriter.WritePlaybackTimes(0, segmentDuration);
                                Logger.Info($"[PlaybackController] Media changed (playing): updated time files to 0s / {segmentDuration}s");
                            }
                            lastMediaFilePath = currentMediaFilePath; // Always update tracker
                        }

                        // Log only when time files are actually updated after a real seek/jump.
                        if (lastKnownVlcPosition >= 0 && Math.Abs(currentPosition - lastKnownVlcPosition) > 2)
                        {
                            NowNextStatusWriter.WritePlaybackTimes(relativePosition, segmentDuration);
                            Logger.Info($"[PlaybackController] Seek detected: updated time files to {relativePosition}s / {segmentDuration}s");
                        }
                        lastKnownVlcPosition = currentPosition;

                        // --- 5-MINUTE AUTO REFRESH LOGIC ---
                        if (!string.IsNullOrEmpty(currentMediaFilePath) && _currentPlaybackState?.State == EventType.Playing)
                        {
                            // Reset on new media
                            if (_lastPolledMediaFilePath == null || !string.Equals(_lastPolledMediaFilePath, currentMediaFilePath, StringComparison.OrdinalIgnoreCase))
                            {
                                _fiveMinutePollCounter = 0;
                                _lastPolledMediaFilePath = currentMediaFilePath;
                            }
                            _fiveMinutePollCounter++;

                            if (_fiveMinutePollCounter >= FiveMinutePollThreshold)
                            {
                                NowNextStatusWriter.WritePlaybackTimes(relativePosition, segmentDuration);
                                _fiveMinutePollCounter = 0;
                                Logger.Info($"[PlaybackController] 5-minute auto-refresh: Updated playback times for '{currentMediaFilePath}'.");
                            }
                        }
                        else
                        {
                            _fiveMinutePollCounter = 0;
                            _lastPolledMediaFilePath = null;
                        }
                        // --- END 5-MINUTE AUTO REFRESH LOGIC ---

                        bool advanced = false;
                        if (currentItem != null && currentItem.NotionLoaded && currentItem.EndTimeSeconds > 0)
                        {
                            if (currentPosition >= currentItem.EndTimeSeconds)
                            {
                                Logger.Info($"[PlaybackController][AUTO-ADVANCE] Notion EndTimeSeconds reached ({currentPosition} >= {currentItem.EndTimeSeconds}), advancing or looping.");
                                await AdvanceToNextOrLoop();
                                advanced = true;
                            }
                        }

                        if (!advanced && _currentPlaybackState.Duration.HasValue && _currentPlaybackState.Duration.Value.TotalSeconds > 10)
                        {
                            double calculatedEnd = _currentPlaybackState.Duration.Value.TotalSeconds - 10;
                            if (currentItem != null && currentPosition >= calculatedEnd)
                            {
                                Logger.Info($"[PlaybackController][AUTO-ADVANCE] Default 10s-before-end reached ({currentPosition} >= {calculatedEnd}), advancing or looping.");
                                await AdvanceToNextOrLoop();
                            }
                        }
                    }

                    if (!_vlcConnected)
                    {
                        _vlcConnected = true;
                        Logger.Info("[PlaybackController] VLC connection established.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("[PlaybackController] PollVlcStatusAsync error occurred.", ex);

                    try { _httpService?.Dispose(); }
                    catch (Exception disposeEx)
                    {
                        Logger.Debug("[PlaybackController] Error disposing HttpService.", disposeEx);
                    }

                    try
                    {
                        _httpService = new HttpService();
                        Logger.Debug("[PlaybackController] Recreated HttpService after connection failure.");
                    }
                    catch (Exception createEx)
                    {
                        Logger.Error("[PlaybackController] Error recreating HttpService.", createEx);
                    }

                    // --- Write "stop" to time files on connection lost ---
                    NowNextStatusWriter.WriteStopToTimeFiles();

                    if (_vlcConnected)
                    {
                        _vlcConnected = false;
                        Logger.Warning("[PlaybackController] VLC connection lost. Notifying searching state.");
                        SetSearchingState();
                    }
                    else
                    {
                        SetSearchingState();
                    }
                }

                await Task.Delay(1000, token);
            }
        }





        private void UpdatePlaybackStateFromXml(string statusXml)
        {
            try
            {
                var doc = XDocument.Parse(statusXml);

                string stateStr = doc.Root.Element("state")?.Value ?? "stopped";
                int.TryParse(doc.Root.Element("time")?.Value, out int position);
                int.TryParse(doc.Root.Element("length")?.Value, out int duration);

                string filename = doc.Descendants("category")
                    .Where(c => (string)c.Attribute("name") == "meta")
                    .Descendants("info")
                    .Where(i => (string)i.Attribute("name") == "filename")
                    .Select(i => i.Value)
                    .FirstOrDefault() ?? "No media";

                int vlcRawVolume = 0;
                int.TryParse(doc.Root.Element("volume")?.Value, out vlcRawVolume);
                int volumePercent = (int)Math.Round(vlcRawVolume * 200.0 / 512.0);
                volumePercent = Math.Max(0, Math.Min(volumePercent, 200));
                _currentPlaybackState.Volume = volumePercent;

                _currentPlaybackState.State = stateStr switch
                {
                    "playing" => EventType.Playing,
                    "paused" => EventType.Paused,
                    "stopped" => EventType.Stopped,
                    _ => EventType.None
                };
                _currentPlaybackState.Position = TimeSpan.FromSeconds(position);
                _currentPlaybackState.Duration = TimeSpan.FromSeconds(duration);

                MediaItem matchedItem = null;
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    matchedItem = _playlist
                        .FirstOrDefault(m =>
                            string.Equals(System.IO.Path.GetFileName(m.FilePath), filename, StringComparison.OrdinalIgnoreCase));
                }
                _currentPlaybackState.CurrentMedia = matchedItem ?? new MediaItem
                {
                    DisplayName = filename,
                    FilePath = filename
                };

                NotifyPlaybackStateChanged(_currentPlaybackState.State);
            }
            catch (Exception ex)
            {
                Logger.Error("[PlaybackController] Error parsing VLC XML.", ex);

                var searchingState = new PlaybackState
                {
                    State = EventType.Searching,
                    CurrentMedia = null,
                    Position = TimeSpan.Zero,
                    Duration = null,
                    IsMuted = false,
                    ErrorMessage = null,
                    Volume = 0
                };
                foreach (var sub in _subscribers)
                {
                    sub.OnPlaybackStateChanged(searchingState);
                }
            }
        }

        public void RegisterSubscriber(IEventSubscriberVLC subscriber)
        {
            if (subscriber == null)
            {
                Logger.Warning("[PlaybackController] Attempted to register null subscriber.");
                return;
            }

            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                Logger.Info($"[PlaybackController] Registered subscriber: {subscriber.GetType().Name}. Total: {_subscribers.Count}");
            }
            else
            {
                Logger.Warning($"[PlaybackController] Subscriber already registered: {subscriber.GetType().Name}");
            }
        }
        public void UnregisterSubscriber(IEventSubscriberVLC subscriber)
        {
            if (subscriber == null)
            {
                Logger.Warning("[PlaybackController] Attempted to unregister null subscriber.");
                return;
            }

            if (_subscribers.Remove(subscriber))
            {
                Logger.Info($"[PlaybackController] Unregistered subscriber: {subscriber.GetType().Name}. Total: {_subscribers.Count}");
            }
            else
            {
                Logger.Warning($"[PlaybackController] Attempted to unregister subscriber not found: {subscriber.GetType().Name}");
            }
        }
        public async Task SetVolumeAsync(int percent)
        {
            int safePercent = Math.Max(0, Math.Min(percent, 200));
            int vlcValue = (int)Math.Round(safePercent * 512.0 / 200.0);

            try
            {
                await _httpService.SendCommandAsync($"volume&val={vlcValue}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[PlaybackController] Failed to set volume to {safePercent}%.", ex);
                throw;
            }
        }
        public bool ContainsMedia(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Warning("[PlaybackController] ContainsMedia called with null or empty filePath.");
                return false;
            }

            bool contains = _playlist.Any(m => string.Equals(m.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            Logger.Debug($"[PlaybackController] ContainsMedia check for '{filePath}' => {contains}");
            return contains;
        }
        public async Task<bool> AdvanceToNextMedia()
        {
            if (_playlist.Count == 0)
            {
                Logger.Info("[PlaybackController] AdvanceToNextMedia called but playlist is empty. Stopping playback.");
                _currentIndex = -1;
                Stop(); // Optionally stop playback if playlist is empty
                return false;
            }

            if (_currentIndex + 1 < _playlist.Count)
            {
                _currentIndex++;
                Logger.Info($"[PlaybackController] Advancing to next media at index {_currentIndex}: {_playlist[_currentIndex].DisplayName}");
                await PlayMediaAtIndex(_currentIndex);
                return true;
            }
            else
            {
                Logger.Info("[PlaybackController] No next media item found. Stopping playback and resetting index.");
                Stop();
                _currentIndex = -1;
                return false;
            }
        }
        private void SetSearchingState()
        {
            Logger.Info("[PlaybackController] SetSearchingState: Notifying all subscribers of searching state.");

            var searchingState = new PlaybackState
            {
                State = EventType.Searching,
                CurrentMedia = null,
                Position = TimeSpan.Zero,
                Duration = null,
                IsMuted = false,
                ErrorMessage = null,
                Volume = 0
            };

            // Optionally update internal state to reflect the last known state is "searching"
            _currentPlaybackState = searchingState;

            // Notify all subscribers
            foreach (var sub in _subscribers)
            {
                sub.OnPlaybackStateChanged(searchingState);
            }
        }
        private async Task AdvanceToNextOrLoop()
        {
            if (_playlist.Count > 1)
            {
                int prevIndex = _currentIndex;
                int nextIndex = (_currentIndex + 1) % _playlist.Count;
                var prevMedia = _playlist[prevIndex];
                var nextMedia = _playlist[nextIndex];
                Logger.Info($"[PlaybackController][AUTO-ADVANCE] Advancing from '{prevMedia.DisplayName}' (index {prevIndex}) to '{nextMedia.DisplayName}' (index {nextIndex}).");
                await PlayMediaAtIndex(nextIndex);
            }
            else if (_playlist.Count == 1)
            {
                var singleMedia = _playlist[0];
                Logger.Info($"[PlaybackController][AUTO-ADVANCE] Looping single playlist item: '{singleMedia.DisplayName}' (index 0).");
                await PlayMediaAtIndex(0);
            }
            else
            {
                Logger.Warning("[PlaybackController][AUTO-ADVANCE] No items in playlist to advance or loop.");
            }
        }

        private async Task FadeVolumeAsync(int fromVolume, int toVolume)
        {
            int fadeDurationMs = 7000; // 7 seconds
            int intervalMs = 50; // Update every 50 ms for smoothness
            int steps = fadeDurationMs / intervalMs;

            double volumeStep = (toVolume - fromVolume) / (double)steps;

            for (int i = 1; i <= steps; i++)
            {
                int currentVolume = (int)Math.Round(fromVolume + volumeStep * i);
                await SetVolumeAsync(currentVolume);
                await Task.Delay(intervalMs);
            }

            // Ensure exact final value
            await SetVolumeAsync(toVolume);
            Logger.Info($"[PlaybackController] Volume fade completed: {fromVolume}% -> {toVolume}% over {fadeDurationMs}ms");
        }
    }
}
