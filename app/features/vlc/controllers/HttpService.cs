using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using theatredeck.app.core.config;
using theatredeck.app.core.logger;

namespace theatredeck.app.features.vlc.controllers
{
    /// <summary>
    /// Handles direct HTTP communication with the VLC HTTP interface.
    /// </summary>
    public class HttpService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _baseUri;

        public HttpService()
        {
            try
            {
                string host = ConfigManager.GetVlcHttpHost();
                int port = ConfigManager.GetVlcHttpPort();
                string password = ConfigManager.GetVlcHttpPassword();

                _baseUri = $"http://{host}:{port}";
                Logger.Info($"Initializing for VLC endpoint: {_baseUri}");

                var handler = new HttpClientHandler();

                if (!string.IsNullOrWhiteSpace(password))
                {
                    handler.Credentials = new NetworkCredential("", password);
                    Logger.Info("Using basic authentication.");
                }

                _client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(_baseUri),
                    Timeout = TimeSpan.FromSeconds(3)
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during initialization.", ex);
                throw;
            }
        }

        /// <summary>
        /// Sends a playback command to VLC.
        /// </summary>
        public async Task SendCommandAsync(string command)
        {
            var requestUri = $"/requests/status.xml?command={command}";
            try
            {
                var response = await _client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending command '{command}'.", ex);
                throw;
            }
        }
        /// <summary>
        /// Gets the current VLC status as XML.
        /// </summary>
        public async Task<string> GetStatusXmlAsync()
        {
            try
            {
                var response = await _client.GetAsync("/requests/status.xml");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Plays a specific media file immediately (replaces current playback).
        /// </summary>
        public async Task PlayMediaAsync(string filePath)
        {
            try
            {
                Logger.Info($"Playing media file: {filePath}");
                string uriPath = Uri.EscapeDataString(filePath);
                string command = $"in_play&input={uriPath}";
                await SendCommandAsync(command);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error playing media file '{filePath}'.", ex);
                throw;
            }
        }
        /// <summary>
        /// Sends the play command to VLC (resume).
        /// </summary>
        public Task PlayAsync()
        {
            Logger.Info("Sending play command.");
            return SendCommandAsync("pl_play");
        }
        /// <summary>
        /// Sends the pause command to VLC.
        /// </summary>
        public Task PauseAsync()
        {
            Logger.Info("Sending pause command.");
            return SendCommandAsync("pl_pause");
        }
        /// <summary>
        /// Sends the stop command to VLC.
        /// </summary>
        public Task StopAsync()
        {
            Logger.Info("Sending stop command.");
            return SendCommandAsync("pl_stop");
        }
        /// <summary>
        /// Seek to a specific position (seconds).
        /// </summary>
        public Task SeekAsync(int seconds)
        {
            int clamped = Math.Max(0, seconds);
            Logger.Info($"Seeking to {clamped} seconds.");
            return SendCommandAsync($"seek&val={clamped}");
        }
        public void Dispose()
        {
            _client?.Dispose();
            Logger.Info("Disposed HttpService.");
        }
    }
}
