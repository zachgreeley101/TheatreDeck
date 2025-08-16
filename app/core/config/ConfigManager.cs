using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace theatredeck.app.core.config
{
    /// <summary>
    /// Centralized application configuration manager.
    /// All configuration access should go through this class—do not use ConfigurationManager directly elsewhere!
    /// </summary>
    public static class ConfigManager
    {
        /// <summary>
        /// Sets or updates an application setting key-value pair in the configuration file and saves the changes.
        /// </summary>
        public static void SetAppSetting(string key, string newValue)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = newValue;
            else
                config.AppSettings.Settings.Add(key, newValue);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Retrieves the value of a specified application setting key, returning "Key not found" if the key does not exist.
        /// </summary>
        public static string GetAppSettingValue(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? "Key not found";
        }

        /// <summary>
        /// Retrieves a configuration value by key, returning a specified default value if the key is not found or is empty.
        /// </summary>
        public static string GetStringConfig(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        /// <summary>
        /// Retrieves an integer configuration value by key, or returns a default if not found or invalid.
        /// </summary>
        public static int GetIntConfigValue(string key, int defaultValue = 0)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets the form title to include the application name and version.
        /// </summary>
        public static void SetFormTextWithAppVersion(Form targetForm)
        {
            if (targetForm == null) throw new ArgumentNullException(nameof(targetForm));
            string applicationName = ConfigurationManager.AppSettings["ApplicationName"] ?? "Application";
            string fullVersion = Application.ProductVersion;
            string simplifiedVersion = fullVersion.Split('+').FirstOrDefault() ?? fullVersion;
            targetForm.Text = $"{applicationName} v{simplifiedVersion}";
        }

        /// <summary>
        /// Retrieves the value of a specified key from a custom configuration file.
        /// </summary>
        public static string GetCustomConfigEntry(string filePath, string key)
        {
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = filePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return config.AppSettings.Settings[key]?.Value ?? "Key not found";
        }

        /// <summary>
        /// Sets or updates a key-value pair in a custom configuration file and saves the changes.
        /// </summary>
        public static void SetCustomConfigEntry(string filePath, string key, string newValue)
        {
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = filePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = newValue;
            else
                config.AppSettings.Settings.Add(key, newValue);

            config.Save(ConfigurationSaveMode.Modified);
        }

        // ===============================
        // VLC-SPECIFIC ACCESSORS
        // ===============================
        public static string GetVlcHttpHost() =>
            GetStringConfig("Vlc_Http_Host", "localhost");

        public static int GetVlcHttpPort() =>
            GetIntConfigValue("Vlc_Http_Port", 8080);

        public static string GetVlcHttpPassword() =>
            GetStringConfig("Vlc_Http_Password");

        public static bool IsVlcConfigValid()
        {
            return !string.IsNullOrWhiteSpace(GetVlcHttpHost())
                   && GetVlcHttpPort() > 0;
        }
    }
}
