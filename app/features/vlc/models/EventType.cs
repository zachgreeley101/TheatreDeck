using System;

namespace theatredeck.app.features.vlc.models
{
    /// <summary>
    /// Defines the types of events that the VLC controller can emit.
    /// </summary>
    public enum EventType
    {
        None = 0,
        Playing,
        Paused,
        Stopped,
        EndReached,
        Error,
        MediaAdded,
        MediaRemoved,
        PositionChanged,
        MediaChanged,
        PlaylistUpdated,
        Searching,
        NowNextUpdated
    }
}
