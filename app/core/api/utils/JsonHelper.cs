using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace theatredeck.app.core.api.utils
{
    public static class JsonHelper
    {
        //===========================================
        // Notion Json Loader
        //===========================================
        /// <summary>
        /// Loads a JSON template and replaces placeholders with provided values.
        /// </summary>
        /// <param name="templatePath">Path to the JSON template file.</param>
        /// <param name="placeholders">Key-value pairs to replace placeholders in the template.</param>
        /// <returns>The processed JSON string with placeholders replaced.</returns>
        public static string LoadAndReplaceNotionTemplate(string templatePath, Dictionary<string, string> placeholders)
        {
            string jsonTemplate = File.ReadAllText(templatePath);

            foreach (var placeholder in placeholders)
            {
                string placeholderKey = $"{{{{{placeholder.Key}}}}}"; // {{Key}}
                string value = placeholder.Value;

                // Replace placeholders with appropriate formatting
                if (bool.TryParse(value, out bool boolValue))
                {
                    jsonTemplate = jsonTemplate.Replace(placeholderKey, boolValue.ToString().ToLower());
                }
                else if (decimal.TryParse(value, out decimal numericValue))
                {
                    jsonTemplate = jsonTemplate.Replace(placeholderKey, numericValue.ToString());
                }
                else
                {
                    jsonTemplate = jsonTemplate.Replace(placeholderKey, value);
                }
            }

            // Replace any unreplaced placeholders with null to maintain valid JSON
            jsonTemplate = Regex.Replace(jsonTemplate, @"\{\{.*?\}\}", "null");

            return jsonTemplate;
        }
        /// <summary>
        /// Loads a JSON query template and replaces placeholders with values.
        /// </summary>
        /// <param name="templateName">Name of the template file located in the "json" folder.</param>
        /// <param name="placeholders">Key-value pairs to replace placeholders in the template.</param>
        /// <returns>The processed JSON string with placeholders replaced.</returns>
        public static string LoadQueryNotionTemplate(string templateName, Dictionary<string, string> placeholders)
        {
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json", templateName);

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Template file not found: {templatePath}");

            var jsonTemplate = File.ReadAllText(templatePath);

            foreach (var placeholder in placeholders)
            {
                jsonTemplate = jsonTemplate.Replace($"{{{placeholder.Key}}}", placeholder.Value);
            }

            return jsonTemplate;
        }
    }
}
