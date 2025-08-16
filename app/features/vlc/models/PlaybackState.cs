using System;

namespace theatredeck.app.features.vlc.models
{
    /// <summary>
    /// Represents the current playback state for the VLC player.
    /// </summary>
    public class PlaybackState
    {
        /// <summary>
        /// Gets or sets the current media item being played.
        /// </summary>
        public MediaItem CurrentMedia { get; set; }

        /// <summary>
        /// Gets or sets the current playback position (in seconds).
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// Gets or sets the total duration of the current media.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the current playback event/state.
        /// </summary>
        public EventType State { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the media is muted.
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Gets or sets any error message encountered during playback.
        /// </summary>
        public string ErrorMessage { get; set; }

        public int Volume { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackState"/> class.
        /// </summary>
        public PlaybackState()
        {
            State = EventType.None;
            Position = TimeSpan.Zero;
        }
    }
}
