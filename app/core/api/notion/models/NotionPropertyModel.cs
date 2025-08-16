using System;
using System.Collections.Generic;
using System.Configuration;

namespace theatredeck.app.core.api.notion.models
{
    public class NotionProperty
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public object? Value { get; set; }

        public object GetFormattedValue()
        {
            if (Value == null)
                throw new InvalidOperationException($"The value for property '{Name}' is null.");

            return Type switch
            {
                "number" => new { number = Convert.ToDouble(Value) },
                "rich_text" => new
                {
                    rich_text = new[]
                    {
                        new { text = new { content = Value.ToString() } }
                    }
                },
                "title" => new
                {
                    title = new[]
                    {
                        new { text = new { content = Value.ToString() } }
                    }
                },
                "checkbox" => new { checkbox = Convert.ToBoolean(Value) },
                "multi_select" => new
                {
                    multi_select = (Value as List<string>)?.ConvertAll(name => new { name })
                },
                _ => throw new ArgumentException($"Unsupported property type: {Type}")
            };
        }
    }

    public class DatabasePropertyModel
    {
        public Dictionary<string, NotionProperty> Properties { get; set; }

        public DatabasePropertyModel()
        {
            Properties = new Dictionary<string, NotionProperty>
        {
            { GetConfigValue("IDSearch_Notion"),      new NotionProperty { Name = "IDSearch", Type = "rich_text" } },
            { GetConfigValue("ServerDrive_Notion"),   new NotionProperty { Name = "Server Drive", Type = "multi_select" } },
            { GetConfigValue("CreatedDate_Notion"),   new NotionProperty { Name = "Created Date", Type = "rich_text" } },
            { GetConfigValue("PlayNext_Notion"),      new NotionProperty { Name = "PlayNext", Type = "checkbox" } },
            { GetConfigValue("Year_Notion"),          new NotionProperty { Name = "Year", Type = "number" } },
            { GetConfigValue("Volume_Notion"),        new NotionProperty { Name = "Volume", Type = "number" } },
            { GetConfigValue("PlayCount_Notion"),     new NotionProperty { Name = "PlayCount", Type = "number" } },
            { GetConfigValue("StartTime_Notion"),     new NotionProperty { Name = "Start Time", Type = "number" } },
            { GetConfigValue("SkippingTime_Notion"),  new NotionProperty { Name = "Skipping Time", Type = "rich_text" } },
            { GetConfigValue("EndTime_Notion"),       new NotionProperty { Name = "End Time", Type = "number" } },
            { GetConfigValue("ModifiedDate_Notion"),  new NotionProperty { Name = "Modified Date", Type = "rich_text" } },
            { GetConfigValue("Location_Notion"),      new NotionProperty { Name = "Location", Type = "rich_text" } },
            { GetConfigValue("ID_Notion"),            new NotionProperty { Name = "ID", Type = "rich_text" } },
            { GetConfigValue("Name_Notion"),          new NotionProperty { Name = "Name", Type = "title" } },
            { GetConfigValue("Tags_Notion"),          new NotionProperty { Name = "Tags", Type = "multi_select" } },
            { GetConfigValue("Collections_Notion"),   new NotionProperty { Name = "Collections", Type = "multi_select" } }
        };
        }

        /// <summary>
        /// Retrieves the property ID from the app.config file.
        /// </summary>
        private string GetConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? throw new KeyNotFoundException($"Key '{key}' not found in app.config.");
        }

        /// <summary>
        /// Generates a dictionary of formatted properties for API requests.
        /// </summary>
        public Dictionary<string, object> GetFormattedProperties()
        {
            var formattedProperties = new Dictionary<string, object>();
            foreach (var property in Properties)
            {
                formattedProperties[property.Key] = property.Value.GetFormattedValue();
            }
            return formattedProperties;
        }
    }

}
