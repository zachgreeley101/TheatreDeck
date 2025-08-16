using theatredeck.app.features.vlc.models;

namespace theatredeck.app.features.vlc.interfaces
{
    /// <summary>
    /// Defines a subscriber for VLC playback events.
    /// Implement this interface to handle state or media updates from the VLC service.
    /// </summary>
    public interface IEventSubscriberVLC
    {
        /// <summary>
        /// Called when the VLC playback state changes.
        /// </summary>
        /// <param name="state">The new playback state.</param>
        void OnPlaybackStateChanged(PlaybackState state);

        /// <summary>
        /// Called when a specific playback event occurs (e.g., EndReached, Error).
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="state">The current playback state at the time of the event.</param>
        void OnPlaybackEvent(EventType eventType, PlaybackState state);
    }
}
