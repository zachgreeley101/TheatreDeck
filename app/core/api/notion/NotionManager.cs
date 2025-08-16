using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using theatredeck.app.core.api.notion.model;
using theatredeck.app.core.api.notion.models;
using theatredeck.app.core.api.utils;
using theatredeck.app.core.config;
using theatredeck.app.core.logger;

namespace theatredeck.app.core.api.notion
{
    public class NotionManager : IDisposable
    {
        private static class Paths // Notion Paths
        {
            public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            public static readonly string JsonDirectory = System.IO.Path.Combine(BaseDirectory, "json");
            public static readonly string QueryDirectory = System.IO.Path.Combine(JsonDirectory, "query");
            public static readonly string DatabaseSchemaFile = System.IO.Path.Combine(QueryDirectory, "DatabaseSchema.json");
            public static readonly string CreateNotionPageTemplate = System.IO.Path.Combine(JsonDirectory, "CreateNotionPageTemplate.json");
        }
        private static class API // Notion API Endpoints
        {
            public static readonly string NotionApiBaseUrl = "https://api.notion.com";
            public static readonly string CreatePageEndpoint = $"{NotionApiBaseUrl}/v1/pages";
            public static string DatabaseEndpoint(string databaseId) => $"{NotionApiBaseUrl}/v1/databases/{databaseId}";
            public static string QueryDatabaseEndpoint(string databaseId) => $"{NotionApiBaseUrl}/v1/databases/{databaseId}/query";
            public static string PageEndpoint(string pageId) => $"{NotionApiBaseUrl}/v1/pages/{pageId}";
        }

        private readonly HttpClient httpClient;
        private readonly string authToken;
        private readonly string databaseId;
        private bool isDisposed;

        public NotionResponse? CurrentResponse { get; private set; }

        //===========================================
        // Initialization & Cleanup
        //===========================================
        public NotionManager(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            authToken = ConfigManager.GetAppSettingValue("NOTION_TOKEN");
            databaseId = ConfigManager.GetAppSettingValue("NOTION_DATABASE_ID");



            if (string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Auth token or database ID is missing.");
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                httpClient.Dispose();
                isDisposed = true;
            }
        }

        //===========================================
        // Main Methods
        //===========================================
        /// <summary>
        /// Fetches the database schema from Notion, saves it as a formatted JSON file,
        /// and returns the JSON string.
        /// </summary>
        public async Task<string> QueryAndSaveDatabaseSchemaAsync()
        {
            try
            {
                // Ensure the query directory exists
                if (!Directory.Exists(Paths.QueryDirectory))
                {
                    Directory.CreateDirectory(Paths.QueryDirectory);
                    Logger.Info($"Created directory: {Paths.QueryDirectory}");
                }

                // Delete existing schema file if it exists
                if (File.Exists(Paths.DatabaseSchemaFile))
                {
                    File.Delete(Paths.DatabaseSchemaFile);
                    Logger.Info($"Existing file '{Paths.DatabaseSchemaFile}' deleted.");
                }

                // Fetch schema from Notion
                string url = API.DatabaseEndpoint(databaseId);
                var response = await ApiHelper.SendNotionGetRequestAsync(httpClient, url, authToken);
                response.EnsureSuccessStatusCode();

                string schemaContent = await response.Content.ReadAsStringAsync();
                var parsedJson = JObject.Parse(schemaContent);
                string formattedJson = parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);

                // Write to file
                File.WriteAllText(Paths.DatabaseSchemaFile, formattedJson);
                Logger.Info($"Database schema fetched and saved to: {Paths.DatabaseSchemaFile}");

                return formattedJson;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to fetch and save database schema: {ex.Message}", ex);
                throw new Exception($"Failed to fetch database schema:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new page in the Notion database using the provided NotionNewPageModel data.
        /// Populates a JSON template with the model's properties and sends a request to Notion API.
        /// </summary>
        /// <param name="notionData">The data model containing the new page information.</param>
        /// <returns>A task representing the asynchronous operation, returning the response as a string.</returns>
        public async Task<string> CreateNotionPageAsync(NotionNewPageModel notionData)
        {
            try
            {
                // Properly build multi-select JSON arrays for ServerDrive, Tags, and Collections
                string serverDriveJson = notionData.ServerDrive != null && notionData.ServerDrive.Any()
                    ? string.Join(", ", notionData.ServerDrive.Select(sd => $"{{ \"name\": \"{sd}\" }}"))
                    : "";

                string tagsJson = notionData.Tags != null && notionData.Tags.Any()
                    ? string.Join(", ", notionData.Tags.Select(tag => $"{{ \"name\": \"{tag}\" }}"))
                    : "";

                string collectionsJson = notionData.Collections != null && notionData.Collections.Any()
                    ? string.Join(", ", notionData.Collections.Select(col => $"{{ \"name\": \"{col}\" }}"))
                    : "";

                var placeholders = new Dictionary<string, string>
        {
            { "ServerDrive", serverDriveJson },
            { "Tags", tagsJson },
            { "Collections", collectionsJson },
            { "PlayNext", notionData.PlayNext },
            { "Year", notionData.Year.ToString() },
            { "Volume", notionData.Volume.ToString() },
            { "PlayCount", notionData.PlayCount.ToString() },
            { "StartTime", notionData.StartTime.ToString() },
            { "SkippingTime", notionData.SkippingTime },
            { "EndTime", notionData.EndTime.ToString() },
            { "Location", notionData.Location },
            { "Name", notionData.Name }
        };

                if (!placeholders.ContainsKey("databaseId"))
                    placeholders["databaseId"] = databaseId;

                string templatePath = Paths.CreateNotionPageTemplate;
                string jsonBody = JsonHelper.LoadAndReplaceNotionTemplate(templatePath, placeholders);

                Logger.Debug("Generated JSON Body:");
                Logger.Debug(jsonBody);

                string url = API.CreatePageEndpoint;
                var response = await ApiHelper.SendNotionPostRequestAsync(httpClient, url, authToken, jsonBody);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"HTTP {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"HTTP {response.StatusCode}: {errorContent}");
                }

                Logger.Info("Notion page created successfully.");
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                Logger.Error($"Failed to create page due to HTTP error: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create page: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Updates properties of an existing Notion page with the given property updates.
        /// Retrieves the necessary database model and formats the values before sending the update request.
        /// </summary>
        /// <param name="pageId">The ID of the Notion page to be updated.</param>
        /// <param name="propertyUpdates">A dictionary containing the properties and their new values.</param>
        public async Task UpdateNotionPagePropertyAsync(string pageId, Dictionary<string, object> propertyUpdates)
        {
            try
            {
                var databaseModel = new DatabasePropertyModel();
                var formattedProperties = new Dictionary<string, object>();

                foreach (var update in propertyUpdates)
                {
                    // Special case: handle multi_select (e.g., Tags)
                    if (update.Key == "Tags" && update.Value is List<string> tagList)
                    {
                        formattedProperties[update.Key] = new
                        {
                            multi_select = tagList.Select(tag => new { name = tag }).ToList()
                        };
                    }
                    else if (databaseModel.Properties.TryGetValue(update.Key, out var notionProperty))
                    {
                        notionProperty.Value = update.Value;
                        formattedProperties[update.Key] = notionProperty.GetFormattedValue();
                    }
                    else
                    {
                        throw new ArgumentException($"Property with ID '{update.Key}' not found in the database model.");
                    }
                }

                var payload = new { properties = formattedProperties };
                string jsonBody = JsonConvert.SerializeObject(payload);
                Logger.Debug("[DEBUG] Patch JSON Body:\n" + jsonBody);

                string url = API.PageEndpoint(pageId);
                var response = await ApiHelper.SendNotionPatchRequestAsync(httpClient, url, authToken, jsonBody);
                response.EnsureSuccessStatusCode();

                Logger.Info("Notion page updated successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to update Notion page: {ex.Message}", ex);
                throw new Exception($"Failed to update Notion page:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Queries the Notion database using a predefined JSON template.
        /// Replaces placeholders in the template and sends a query request to Notion API.
        /// </summary>
        /// <param name="templateName">The name of the query template to use.</param>
        /// <param name="placeholders">Optional placeholders to replace in the template.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of NotionDatabaseModel instances.</returns>
        public async Task<List<NotionDatabaseModel>> QueryNotionDatabaseAsync(string templateName, Dictionary<string, string>? placeholders = null)
        {
            try
            {
                placeholders ??= new Dictionary<string, string>();
                if (!placeholders.ContainsKey("databaseId"))
                    placeholders["databaseId"] = databaseId;

                string jsonBody = JsonHelper.LoadQueryNotionTemplate(templateName, placeholders);
                string url = API.QueryDatabaseEndpoint(databaseId);

                var response = await ApiHelper.SendNotionPostRequestAsync(httpClient, url, authToken, jsonBody);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var notionResponse = JsonConvert.DeserializeObject<NotionResponse>(responseContent);

                // Convert the raw API response into domain model instances.
                List<NotionDatabaseModel> models = ConvertResponseToModels(notionResponse);

                Logger.Info($"Successfully queried database. Returned {models.Count} records.");
                return models;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to query database: {ex.Message}", ex);
                MessageBox.Show($"Failed to query database:\n{ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                throw;
            }
        }

        public async Task<string> QueryAndSaveDatabaseResultAsync(string templateName, Dictionary<string, string>? placeholders = null)
        {
            try
            {
                // Ensure the query directory exists
                if (!Directory.Exists(Paths.QueryDirectory))
                {
                    Directory.CreateDirectory(Paths.QueryDirectory);
                    Logger.Info($"Created directory: {Paths.QueryDirectory}");
                }

                // Set result file path (customize if needed)
                string resultFile = Path.Combine(Paths.QueryDirectory, "NotionDatabaseResult.json");

                // Delete existing file if it exists
                if (File.Exists(resultFile))
                {
                    File.Delete(resultFile);
                    Logger.Info($"Existing file '{resultFile}' deleted.");
                }

                placeholders ??= new Dictionary<string, string>();
                if (!placeholders.ContainsKey("databaseId"))
                    placeholders["databaseId"] = databaseId;

                string jsonBody = JsonHelper.LoadQueryNotionTemplate(templateName, placeholders);
                string url = API.QueryDatabaseEndpoint(databaseId);

                var response = await ApiHelper.SendNotionPostRequestAsync(httpClient, url, authToken, jsonBody);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var parsedJson = JObject.Parse(responseContent);
                string formattedJson = parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);

                // Write to file
                File.WriteAllText(resultFile, formattedJson);
                Logger.Info($"Database query result fetched and saved to: {resultFile}");

                return formattedJson;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to fetch and save database query result: {ex.Message}", ex);
                throw new Exception($"Failed to fetch database query result:\n{ex.Message}");
            }
        }

        //===========================================
        // Services
        //===========================================
        /// <summary>
        /// Converts the raw Notion API response into a list of NotionDatabaseModel instances.
        /// Extracts relevant data fields and formats them appropriately.
        /// </summary>
        /// <param name="response">The raw API response containing database query results.</param>
        /// <returns>A list of NotionDatabaseModel instances with extracted data.</returns>
        public List<NotionDatabaseModel> ConvertResponseToModels(NotionResponse response)
        {
            if (response?.Results == null || !response.Results.Any())
            {
                Logger.Info("[NotionManager] No results found in the response.");
                return new List<NotionDatabaseModel>();
            }

            var models = response.Results.Select(page => new NotionDatabaseModel
            {
                // New: Capture the Notion page's unique ID
                PageId = page.Id,
                IDSearch = page.Properties?.IDSearch?.Formula?.String ?? string.Empty,
                ServerDrive = page.Properties?.ServerDrive?.MultiSelect?.Select(sd => sd.Name ?? string.Empty).ToList() ?? new List<string>(),
                CreatedDate = page.Properties?.CreatedDate?.CreatedTime,
                PlayNext = page.Properties?.PlayNext?.Checkbox ?? false,
                Year = page.Properties?.Year?.Number is double y ? (int?)y : null,
                Volume = page.Properties?.Volume?.Number is double v ? (int?)v : null,
                PlayCount = page.Properties?.PlayCount?.Number is double p ? (int?)p : null,
                StartTime = page.Properties?.StartTime?.Number is double s ? (int?)s : null,
                SkippingTime = page.Properties?.SkippingTime?.RichText?.FirstOrDefault()?.PlainText ?? string.Empty,
                EndTime = page.Properties?.EndTime?.Number is double e ? (int?)e : null,
                ModifiedDate = page.Properties?.ModifiedDate?.LastEditedTime,
                Location = page.Properties?.Location?.RichText?.FirstOrDefault()?.PlainText ?? string.Empty,
                ID = (page.Properties?.ID?.UniqueId?.Prefix ?? "") + (page.Properties?.ID?.UniqueId?.Number?.ToString() ?? string.Empty),
                Name = page.Properties?.Name?.Title?.FirstOrDefault()?.PlainText ?? string.Empty,
                Tags = page.Properties?.Tags?.MultiSelect?.Select(t => t.Name ?? string.Empty).ToList() ?? new List<string>(),
                Collections = page.Properties?.Collections?.MultiSelect?.Select(c => c.Name ?? string.Empty).ToList() ?? new List<string>()
            }).ToList();

            DebugCurrentModel(models);

            return models;
        }


        /// <summary>
        /// Logs the details of the Notion database models for debugging purposes.
        /// </summary>
        /// <param name="models">The list of NotionDatabaseModel instances to log.</param>
        public void DebugCurrentModel(List<NotionDatabaseModel> models)
        {
            if (models == null || !models.Any())
            {
                Logger.Debug("[NotionManager] No models available to log.");
                return;
            }

            foreach (var model in models)
            {
                Logger.Debug("-------- CURRENT THEATRE MODEL ------------");
                Logger.Debug($"PageId:        {model.PageId}");
                Logger.Debug($"IDSearch:      {model.IDSearch}");
                Logger.Debug($"ServerDrive:   {string.Join(", ", model.ServerDrive ?? new List<string>())}");
                Logger.Debug($"CreatedDate:   {model.CreatedDate}");
                Logger.Debug($"PlayNext:      {model.PlayNext}");
                Logger.Debug($"Year:          {model.Year}");
                Logger.Debug($"Volume:        {model.Volume}");
                Logger.Debug($"PlayCount:     {model.PlayCount}");
                Logger.Debug($"StartTime:     {model.StartTime}");
                Logger.Debug($"SkippingTime:  {model.SkippingTime}");
                Logger.Debug($"EndTime:       {model.EndTime}");
                Logger.Debug($"ModifiedDate:  {model.ModifiedDate}");
                Logger.Debug($"Location:      {model.Location}");
                Logger.Debug($"ID:            {model.ID}");
                Logger.Debug($"Name:          {model.Name}");
                Logger.Debug($"Tags:          {string.Join(", ", model.Tags ?? new List<string>())}");
                Logger.Debug($"Collections:   {string.Join(", ", model.Collections ?? new List<string>())}");
                Logger.Debug("-------------------------------------------");
            }
        }


        //===========================================
        // Events
        //===========================================
        /// <summary>
        /// Updates the PlayNext property of a Notion page.
        /// </summary>
        /// <param name="pageId">The ID of the Notion page to update.</param>
        /// <param name="playNext">The new value for the PlayNext property.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task UpdatePlayNextAsync(string pageId, bool playNext)
        {
            if (string.IsNullOrWhiteSpace(pageId))
                throw new ArgumentException("Page ID cannot be null or whitespace", nameof(pageId));

            // Retrieve the PlayNext property ID from configuration.
            string playNextPropertyId = ConfigManager.GetAppSettingValue("PlayNext_Notion");
            if (string.IsNullOrEmpty(playNextPropertyId))
            {
                Logger.Warning("[NotionManager] PlayNext property ID is not configured.");
                return;
            }

            // Build the dictionary of property updates.
            var propertyUpdates = new Dictionary<string, object>
    {
        { playNextPropertyId, playNext }
    };

            try
            {
                // Update the Notion page using the existing method.
                await UpdateNotionPagePropertyAsync(pageId, propertyUpdates);
                Logger.Info("[NotionManager] PlayNext property updated successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[NotionManager] Error updating PlayNext property: {ex.Message}", ex);
                throw;
            }
        }
    }
}