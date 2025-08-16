using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using theatredeck.app.core.api.obs.services;
using theatredeck.app.core.api.obs.utils;
using theatredeck.app.core.logger;

namespace theatredeck.app.core.api.obs
{
    public class OBSManager
    {
        private readonly OBSConnection _clientConnection;

        public OBSManager()
        {
            _clientConnection = new OBSConnection();
        }

        private readonly Dictionary<string, CancellationTokenSource> _autoConnectTokens = new();

        //======================================
        // Connection
        //======================================
        public async Task StartScreenOBSConnection()
        {
            try
            {
                await _clientConnection.ConnectAsync("Screen");
                Logger.Info("OBS Screen connection started successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error starting OBS Screen connection.", ex);
            }
        }
        public async Task StartCameraOBSConnection()
        {
            try
            {
                await _clientConnection.ConnectAsync("Camera");
                Logger.Info("OBS Camera connection started successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error starting OBS Camera connection.", ex);
            }
        }
        public async Task StopOBSConnection()
        {
            try
            {
                await _clientConnection.DisconnectAsync();
                Logger.Info("OBS connection stopped successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error stopping OBS connection.", ex);
            }
        }
        public void StartAutoConnect(string connectionType)
        {
            if (_autoConnectTokens.ContainsKey(connectionType))
            {
                Logger.Warning($"AutoConnect for {connectionType} is already running.");
                return;
            }

            var tokenSource = new CancellationTokenSource();
            _autoConnectTokens[connectionType] = tokenSource;

            Logger.Info($"AutoConnect for {connectionType} started.");
            Task.Run(() => AutoConnectOBS(connectionType, tokenSource.Token));
        }
        public void StopAutoConnect(string connectionType)
        {
            if (_autoConnectTokens.TryGetValue(connectionType, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                _autoConnectTokens.Remove(connectionType);
                Logger.Info($"AutoConnect for {connectionType} stopped.");
            }
            else
            {
                Logger.Warning($"AutoConnect for {connectionType} was not running.");
            }
        }
        private async Task AutoConnectOBS(string connectionType, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_clientConnection._webSockets[connectionType].State != WebSocketState.Open)
                    {
                        Logger.Debug($"Attempting to auto-connect to OBS {connectionType} WebSocket...");
                        await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"AutoConnect error ({connectionType}).", ex);
                }

                await Task.Delay(5000, cancellationToken);
            }

            Logger.Debug($"AutoConnect loop for {connectionType} exited.");
        }
        //======================================
        // Actions
        //======================================

        // connectionType = Screen or Camera
        public async Task EnableSourceVisibility(string connectionType, string sceneName, string sourceName, CancellationToken cancellationToken = default)
        {
            try
            {
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];
                while (webSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(100, cancellationToken);
                }

                int sceneItemId = await OBS_Helper.RetryGetSceneItemId(_clientConnection, connectionType, sceneName, sourceName, cancellationToken);

                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "SetSceneItemEnabled",
                        requestId,
                        requestData = new
                        {
                            sceneName,
                            sceneItemId,
                            sceneItemEnabled = true
                        }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                // Optionally, wait for a confirmation response matching requestId here.

                Logger.Info($"Source '{sourceName}' in scene '{sceneName}' has been enabled successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error enabling source '{sourceName}' in scene '{sceneName}'.", ex);
            }
        }
        public async Task DisableSourceVisibility(string connectionType, string sceneName, string sourceName, CancellationToken cancellationToken = default)
        {
            try
            {
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];
                while (webSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(100, cancellationToken);
                }

                int sceneItemId = await OBS_Helper.RetryGetSceneItemId(_clientConnection, connectionType, sceneName, sourceName, cancellationToken);

                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "SetSceneItemEnabled",
                        requestId,
                        requestData = new
                        {
                            sceneName,
                            sceneItemId,
                            sceneItemEnabled = false
                        }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                // Optionally, wait for a confirmation response matching requestId here.

                Logger.Info($"Source '{sourceName}' in scene '{sceneName}' has been disabled successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error disabling source '{sourceName}' in scene '{sceneName}'.", ex);
            }
        }
        public async Task ToggleSourceVisibility(string connectionType, string sceneName, string sourceName)
        {
            try
            {
                // Ensure WebSocket connection is established
                await _clientConnection.ConnectAsync(connectionType);
                var webSocket = _clientConnection._webSockets[connectionType];

                // Wait until the connection is fully open
                while (webSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(100);
                }

                // Retrieve Scene Item ID
                int sceneItemId = await OBS_Helper.RetryGetSceneItemId(_clientConnection, connectionType, sceneName, sourceName);

                // Construct payload to get the current visibility state
                string checkRequestId = Guid.NewGuid().ToString();
                var visibilityCheckPayload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "GetSceneItemEnabled",
                        requestId = checkRequestId,
                        requestData = new
                        {
                            sceneName,
                            sceneItemId
                        }
                    }
                };

                // Send request to get current state
                var jsonPayload = JsonSerializer.Serialize(visibilityCheckPayload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload);

                // Loop until we get the matching response
                bool currentState = false;
                while (true)
                {
                    var response = await _clientConnection.ReceiveMessageAsync(webSocket);
                    if (string.IsNullOrWhiteSpace(response))
                        continue; // ignore empty

                    try
                    {
                        var document = JsonDocument.Parse(response);
                        if (document.RootElement.TryGetProperty("d", out var dElem) &&
                            dElem.TryGetProperty("requestId", out var respRequestIdElem) &&
                            respRequestIdElem.GetString() == checkRequestId)
                        {
                            // We found the response matching our requestId
                            bool enabled = dElem
                                .GetProperty("responseData")
                                .GetProperty("sceneItemEnabled")
                                .GetBoolean();
                            currentState = enabled;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Error parsing response for ToggleSourceVisibility. Raw message: {response}", ex);
                    }
                }

                // Toggle the visibility
                bool newState = !currentState;
                var toggleRequestId = Guid.NewGuid().ToString();
                var togglePayload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "SetSceneItemEnabled",
                        requestId = toggleRequestId,
                        requestData = new
                        {
                            sceneName,
                            sceneItemId,
                            sceneItemEnabled = newState
                        }
                    }
                };

                // Send toggle request
                var toggleJson = JsonSerializer.Serialize(togglePayload);
                await _clientConnection.SendMessageAsync(webSocket, toggleJson);

                Logger.Info($"Source '{sourceName}' in scene '{sceneName}' visibility toggled to: {newState}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error toggling visibility for '{sourceName}' in scene '{sceneName}'.", ex);
            }
        }
        public async Task UpdateTextSource(string connectionType, string sceneName, string sourceName, string updatedText, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ensure WebSocket connection is established
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];
                while (webSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(100, cancellationToken);
                }

                // Retry fetching Scene Item ID
                int sceneItemId = await OBS_Helper.RetryGetSceneItemId(_clientConnection, connectionType, sceneName, sourceName, cancellationToken);

                // Construct the payload to update the Text+ source
                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "SetInputSettings",
                        requestId,
                        requestData = new
                        {
                            inputName = sourceName,
                            inputSettings = new
                            {
                                text = updatedText
                            },
                            overlay = false
                        }
                    }
                };

                // Send update request
                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                Logger.Info($"Text+ source '{sourceName}' in scene '{sceneName}' has been updated to '{updatedText}'.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating Text+ source '{sourceName}' in scene '{sceneName}'.", ex);
            }
        }
        public async Task SwitchScene(string connectionType, string sceneName, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ensure the connection is established.
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];
                while (webSocket.State != WebSocketState.Open)
                {
                    await Task.Delay(100, cancellationToken);
                }

                // Create a unique requestId.
                string requestId = Guid.NewGuid().ToString();

                // Create the payload for switching scene.
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "SetCurrentProgramScene", // Use "SetCurrentScene" for older OBS versions
                        requestId,
                        requestData = new
                        {
                            sceneName
                        }
                    }
                };

                // Serialize and send the payload.
                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                Logger.Info($"Switched to scene '{sceneName}' successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error switching scene to '{sceneName}'.", ex);
            }
        }
        public async Task RefreshBrowserSource(string connectionType, string sourceName, CancellationToken cancellationToken = default)
        {
            try
            {
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];

                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "PressInputPropertiesButton",
                        requestId,
                        requestData = new
                        {
                            inputName = sourceName,
                            propertyName = "refresh"
                        }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                Logger.Info($"Browser source '{sourceName}' refreshed.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error refreshing browser source '{sourceName}'.", ex);
            }
        }
        public async Task CaptureSourceScreenshot(string connectionType, string sourceName, string savePath, CancellationToken cancellationToken = default)
        {
            try
            {
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];

                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "SaveSourceScreenshot",
                        requestId,
                        requestData = new
                        {
                            sourceName,
                            imageFormat = "png",
                            savePath
                        }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                Logger.Info($"Screenshot of source '{sourceName}' saved to '{savePath}'.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error capturing screenshot of '{sourceName}'.", ex);
            }
        }
        public async Task ToggleRecording(string connectionType, CancellationToken cancellationToken = default)
        {
            try
            {
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];

                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "ToggleRecord",
                        requestId
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                Logger.Info("Toggled recording.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error toggling recording.", ex);
            }
        }
        public async Task ToggleMuteSource(string connectionType, string sourceName, CancellationToken cancellationToken = default)
        {
            try
            {
                await _clientConnection.ConnectAsync(connectionType, cancellationToken);
                var webSocket = _clientConnection._webSockets[connectionType];

                string requestId = Guid.NewGuid().ToString();
                var payload = new
                {
                    op = 6,
                    d = new
                    {
                        requestType = "ToggleInputMute",
                        requestId,
                        requestData = new
                        {
                            inputName = sourceName
                        }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                await _clientConnection.SendMessageAsync(webSocket, jsonPayload, cancellationToken);

                Logger.Info($"Toggled mute for source '{sourceName}'.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error toggling mute for '{sourceName}'.", ex);
            }
        }
    }
}
