using System;
using System.Collections.Generic;
using System.Threading.Tasks; // <-- Be sure to include this
using theatredeck.app.features.vlc.models;

namespace theatredeck.app.features.vlc.interfaces
{
    /// <summary>
    /// Defines the contract for VLC playback service.
    /// </summary>
    public interface IServiceVLC
    {
        // Core playback controls (async)
        Task Play();
        Task Pause();
        Task Stop();

        // Media/Playlist controls (async where necessary)
        Task AddMedia(MediaItem item); // If you want to make AddMedia async (recommended)
        Task RemoveMedia(MediaItem item);
        Task ClearPlaylist();          // If you want ClearPlaylist async
        Task<int> MoveMediaUp(int index);
        Task<int> MoveMediaDown(int index);

        // Playback status
        PlaybackState GetCurrentState();
        IList<MediaItem> GetPlaylist();

        // Event subscription (observer pattern)
        void Subscribe(IEventSubscriberVLC subscriber);
        void Unsubscribe(IEventSubscriberVLC subscriber);

        // Optional advanced methods
        Task Seek(TimeSpan position);
        Task PlayMediaAtIndex(int index);
    }
}
