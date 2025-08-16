using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace theatredeck.app.core.api.utils
{
    public static class ApiHelper
    {
        //===========================================
        // Notion API Helpers
        //===========================================

        /// <summary>
        /// Sends a GET request to the Notion API with authorization headers.
        /// </summary>
        public static async Task<HttpResponseMessage> SendNotionGetRequestAsync(HttpClient client, string url, string authToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddNotionAuthorizationHeader(request, authToken);
            return await client.SendAsync(request);
        }
        /// <summary>
        /// Sends a POST request to the Notion API with authorization headers and JSON content.
        /// </summary>
        public static async Task<HttpResponseMessage> SendNotionPostRequestAsync(HttpClient client, string url, string authToken, string jsonBody)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            AddNotionAuthorizationHeader(request, authToken);
            return await client.SendAsync(request);
        }
        /// <summary>
        /// Adds Notion-specific authorization and version headers to the HTTP request.
        /// </summary>
        private static void AddNotionAuthorizationHeader(HttpRequestMessage request, string authToken)
        {
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            request.Headers.Add("Notion-Version", "2022-06-28");
        }
        /// <summary>
        /// Waits for a Notion page or resource to become available by retrying with a specified delay and maximum attempts.
        /// </summary>
        public static async Task<string?> WaitForNotionPageCreationAsync(Func<Task<string?>> fetchPageFunction,int maxAttempts = 5,int delayMilliseconds = 1000)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    // Try to fetch the page
                    string? result = await fetchPageFunction();

                    // If found, return the result
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception but continue retrying
                    Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
                }

                // Wait before retrying
                await Task.Delay(delayMilliseconds);
            }

            // If not found after retries, throw a timeout exception
            throw new TimeoutException($"Failed to retrieve the resource after {maxAttempts} attempts.");
        }
        /// <summary>
        /// Sends a PATCH request to the Notion API with the provided JSON payload.
        /// Adds the necessary authorization headers before making the request.
        /// </summary>
        /// <param name="client">The HttpClient instance used to send the request.</param>
        /// <param name="url">The Notion API endpoint URL.</param>
        /// <param name="authToken">The authorization token for Notion API access.</param>
        /// <param name="jsonBody">The JSON payload to be sent in the request.</param>
        /// <returns>A task representing the asynchronous operation, returning the HTTP response.</returns>
        public static async Task<HttpResponseMessage> SendNotionPatchRequestAsync(HttpClient client, string url, string authToken, string jsonBody)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            AddNotionAuthorizationHeader(request, authToken);
            return await client.SendAsync(request);
        }
    }
}
