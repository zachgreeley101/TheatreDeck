using System;
using theatredeck.app.features.vlc.controllers;
using theatredeck.app.features.vlc.interfaces;
using theatredeck.app.core.config;
using theatredeck.app.core.logger; // <-- Added for Logger
using theatredeck.app.core.api.notion;

namespace theatredeck.app.features.vlc
{
    /// <summary>
    /// Responsible for initializing the VLC feature and wiring up required components.
    /// </summary>
    public class FeatureBootstrapperVLC
    {
        public PlaybackController PlaybackController { get; private set; }

        // Updated constructor to accept NotionManager
        public FeatureBootstrapperVLC(NotionManager notionManager)
        {
            try
            {
                Logger.Info("Initializing VLC feature...");

                if (notionManager == null)
                {
                    Logger.Error("NotionManager is null!");
                    throw new ArgumentNullException(nameof(notionManager));
                }

                PlaybackController = new PlaybackController(notionManager);

                if (!ConfigManager.IsVlcConfigValid())
                {
                    Logger.Error("Invalid VLC configuration detected.");
                    throw new InvalidOperationException("VLC configuration is missing or invalid. Check your App.config settings.");
                }

                Logger.Info("VLC feature initialized successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during initialization.", ex);
                throw;
            }
        }

        /// <summary>
        /// Registers the UI or other subscribers to receive VLC playback events.
        /// </summary>
        public void RegisterSubscriber(IEventSubscriberVLC subscriber)
        {
            if (subscriber != null)
            {
                PlaybackController.RegisterSubscriber(subscriber);
                Logger.Info($"Registered subscriber: {subscriber.GetType().Name} (PlaybackController only)");
            }
            else
            {
                Logger.Warning("Attempted to register null subscriber.");
            }
        }

        /// <summary>
        /// Unregisters a subscriber from VLC event notifications.
        /// </summary>
        public void UnregisterSubscriber(IEventSubscriberVLC subscriber)
        {
            if (subscriber != null)
            {
                PlaybackController.UnregisterSubscriber(subscriber);
                Logger.Info($"Unregistered subscriber: {subscriber.GetType().Name}");
            }
            else
            {
                Logger.Warning("Attempted to unregister null subscriber.");
            }
        }
    }
}
