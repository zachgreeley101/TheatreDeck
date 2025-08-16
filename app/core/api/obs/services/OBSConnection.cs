using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using theatredeck.app.core.config;
using theatredeck.app.core.logger;

namespace theatredeck.app.core.api.obs.services
{
    public class OBSConnection
    {
        public readonly Dictionary<string, ClientWebSocket> _webSockets;
        private readonly Dictionary<string, string> _connectionUrls;
        private readonly Dictionary<string, string> _authTokens;

        public OBSConnection()
        {
            // Initialize dictionaries
            _webSockets = new Dictionary<string, ClientWebSocket>
    {
        { "Screen", new ClientWebSocket() },
        { "Camera", new ClientWebSocket() }
    };

            _connectionUrls = new Dictionary<string, string>
    {
        { "Screen", $"ws://localhost:{ConfigManager.GetStringConfig("OBS_ScreenPort", "4455")}" },
        { "Camera", $"ws://localhost:{ConfigManager.GetStringConfig("OBS_CameraPort", "4456")}" }
    };

            _authTokens = new Dictionary<string, string>
    {
        { "Screen", ConfigManager.GetCustomConfigEntry("Secrets.config", "OBS_ScreenToken") },
        { "Camera", ConfigManager.GetCustomConfigEntry("Secrets.config", "OBS_CameraToken") }
    };

            foreach (var type in _connectionUrls.Keys)
            {
                // Log URLs for debugging but do not log sensitive tokens
                Logger.Info($"WebSocket URL for {type}: {_connectionUrls[type]}");
                Logger.Debug($"Authentication token retrieved for {type}. (Token value is not logged for security reasons.)");
            }
        }

        /// <summary>
        /// Establishes a WebSocket connection for the specified connection type.
        /// If the WebSocket is already connected, it returns immediately.
        /// Otherwise, it attempts to connect, handling reconnections if necessary.
        /// </summary>
        /// <param name="connectionType">The type of WebSocket connection (e.g., OBS).</param>
        /// <param name="cancellationToken">Optional cancellation token for async operations.</param>
        public async Task ConnectAsync(string connectionType, CancellationToken cancellationToken = default)
        {
            if (!_webSockets.ContainsKey(connectionType))
                throw new ArgumentException($"Invalid connection type: {connectionType}");

            var webSocket = _webSockets[connectionType];
            var webSocketUrl = _connectionUrls[connectionType];
            var authToken = _authTokens[connectionType];

            try
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    Logger.Info($"{connectionType} WebSocket is already connected.");
                    return;
                }

                if (webSocket.State != WebSocketState.None)
                {
                    webSocket.Dispose();
                    _webSockets[connectionType] = new ClientWebSocket();
                    webSocket = _webSockets[connectionType];
                    Logger.Warning($"Reinitialized WebSocket for {connectionType}.");
                }

                Logger.Info($"Attempting to connect to OBS WebSocket ({connectionType}) at {webSocketUrl}...");
                await webSocket.ConnectAsync(new Uri(webSocketUrl), cancellationToken);

                if (webSocket.State == WebSocketState.Open)
                {
                    Logger.Info($"Successfully connected to OBS WebSocket for {connectionType} at {webSocketUrl}.");
                    await AuthenticateAsync(webSocket, authToken, cancellationToken);
                }
                else
                {
                    Logger.Warning($"Connection attempt to {connectionType} OBS WebSocket failed.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error connecting to OBS WebSocket for {connectionType}: {ex.Message}", ex);
                await Task.Delay(500, cancellationToken);
                if (webSocket.State != WebSocketState.Open)
                {
                    Logger.Warning($"Retrying OBS WebSocket connection for {connectionType}...");
                    await ConnectAsync(connectionType, cancellationToken);
                }
            }
        }
        /// <summary>
        /// Authenticates the WebSocket connection using a challenge-response mechanism.
        /// Extracts authentication details from the received Hello message and sends the appropriate response.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection to authenticate.</param>
        /// <param name="authToken">The authentication token used for generating the response.</param>
        /// <param name="cancellationToken">Optional cancellation token for async operations.</param>
        private async Task AuthenticateAsync(ClientWebSocket webSocket, string authToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (webSocket.State != WebSocketState.Open)
                {
                    throw new InvalidOperationException("WebSocket connection is not active. Please connect before attempting authentication.");
                }

                var helloMessage = await ReceiveMessageAsync(webSocket, cancellationToken);
                if (string.IsNullOrEmpty(helloMessage))
                {
                    Logger.Warning("Failed to receive Hello message.");
                    return;
                }

                var document = JsonDocument.Parse(helloMessage);
                if (document.RootElement.TryGetProperty("d", out var dataElement) &&
                    dataElement.TryGetProperty("authentication", out var authElement))
                {
                    var authChallenge = authElement.GetProperty("challenge").GetString();
                    var salt = authElement.GetProperty("salt").GetString();

                    var authResponse = HashPassword(HashPassword(authToken, salt), authChallenge);
                    var payload = JsonSerializer.Serialize(new
                    {
                        op = 1,
                        d = new { rpcVersion = 1, authentication = authResponse }
                    });

                    await SendMessageAsync(webSocket, payload, cancellationToken);
                }
                else
                {
                    Logger.Warning("Authentication fields missing or invalid Hello message format.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during authentication: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Computes a hashed password using SHA-256, combining the provided input with a salt.
        /// </summary>
        /// <param name="input">The input string (e.g., auth token).</param>
        /// <param name="salt">The salt to append to the input before hashing.</param>
        /// <returns>A Base64-encoded SHA-256 hash of the input.</returns>
        private static string HashPassword(string input, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(input + salt);
            return Convert.ToBase64String(sha256.ComputeHash(combined));
        }
        /// <summary>
        /// Receives a message from the WebSocket asynchronously, handling fragmented messages.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection.</param>
        /// <param name="cancellationToken">Optional cancellation token for async operations.</param>
        /// <returns>The received message as a string.</returns>
        public async Task<string> ReceiveMessageAsync(ClientWebSocket webSocket, CancellationToken cancellationToken = default)
        {
            Logger.Debug("ReceiveMessageAsync started.");

            var buffer = new byte[4096];
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    Logger.Debug($"Received {result.Count} bytes, EndOfMessage={result.EndOfMessage}");
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                var message = Encoding.UTF8.GetString(ms.ToArray());
                Logger.Debug($"Complete message received: {message.Length} characters.");
                return message;
            }
        }
        /// <summary>
        /// Sends a message to the WebSocket asynchronously, ensuring the connection is open before sending.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Optional cancellation token for async operations.</param>
        public async Task SendMessageAsync(ClientWebSocket webSocket, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (webSocket == null || webSocket.State != WebSocketState.Open)
                {
                    Logger.Warning("Cannot send message. WebSocket is either null or not connected.");
                    return;
                }

                Logger.Debug($"Preparing to send message to OBS. Message length: {message?.Length ?? 0} characters.");
                var buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
                Logger.Info("Message successfully sent to OBS.");
            }
            catch (WebSocketException wsEx)
            {
                Logger.Error($"WebSocketException while sending message: {wsEx.Message}", wsEx);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error sending message: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Closes and disposes all active WebSocket connections.
        /// Ensures a graceful shutdown by closing each WebSocket properly before resetting it.
        /// </summary>
        public async Task DisconnectAsync()
        {
            foreach (var connectionType in _webSockets.Keys.ToList())
            {
                var webSocket = _webSockets[connectionType];
                try
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                        Logger.Info($"Disconnected from OBS WebSocket for {connectionType}.");
                    }
                    _webSockets[connectionType].Dispose();
                    _webSockets[connectionType] = new ClientWebSocket(); // Reset WebSocket for reuse
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error disconnecting {connectionType}: {ex.Message}", ex);
                }
            }
        }
    }
}