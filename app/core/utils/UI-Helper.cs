using System.Configuration;
using System.Diagnostics;
using theatredeck.app.core.config;
using theatredeck.app.core.logger;
using theatredeck.app.forms;

namespace theatredeck.app.core.utils
{
    public static class UI_Helper
    {
        //===========================================
        // Main Logic
        //===========================================


        //===========================================
        // Updated Label Calls
        //===========================================











        //===========================================
        // Combo Box Items
        //===========================================
        /// <summary>
        /// Returns a list of available playlist items.
        /// </summary>
        /// <returns>A List of playlist names.</returns>
        public static List<string> GetPlaylistItems()
        {
            return new List<string>
            {
                "Playlist",
                "80s Wave",
                "Retrowave",
                "CyberCore"
            };
        }

        /// <summary>
        /// Returns a list of available BPM algorithm keys.
        /// These values correspond to those accepted by TimedAlgorithmFactory.
        /// </summary>
        public static List<string> GetBPMAlgorithmList()
        {
            return new List<string>
            {
                "neon_wave",
                "shuffle_rest(.5)",
                "shuffle(.5)",
                "large_wave(4)",
                "medium_wave(2)",
                "small_wave(1)",
                "calibration_consistent",
                "calibration_energyup",
                "calibration_energydown",
                "calibration_wavepattern",
                "calibration_randomized",
                "calibration_steppattern",
                "calibration_sawtooth",
                "calibration_heartbeat",
                "calibration_ladder",
                "calibration_randomwalk",
                "calibration_mountain",
                "calibration_sprintrest",
                "calibration_doubledip",
                "calibration_plateau"
            };
        }
        /// <summary>
        /// Returns the available operation modes for the comboBoxOperationMode.
        /// </summary>
        public static List<string> GetOperationModeList()
        {
            return new List<string>
            {
                "Default",
                "Vol-Check"
            };
        }


        //===========================================
        // Utils Methods
        //===========================================
        /// <summary>
        /// Automatically applies default selections to recognized ComboBoxes based on app.config values.
        /// </summary>
        public static void ApplyDefaultComboBoxSettings(Form mainForm)
        {
            if (mainForm == null) return;

            ConfigurationManager.RefreshSection("appSettings");

            var neonForm = mainForm as TheatreDeckForm;
            if (neonForm != null)
                neonForm.IsInitializingDefaults = true;

            foreach (ComboBox cb in GetAllComboBoxes(mainForm))
            {
                if (cb.Name == "comboBoxPlaylist")
                    cb.DataSource = GetPlaylistItems();

                if (cb.Name == "comboBoxBPMAlgorithm")
                    cb.DataSource = GetBPMAlgorithmList();

                if (cb.Name == "comboBoxOperationMode")
                {
                    cb.DataSource = GetOperationModeList();

                    string defaultMode = ConfigurationManager.AppSettings["CBDefaultOperationMode"];
                    if (!string.IsNullOrEmpty(defaultMode))
                    {
                        int index = cb.Items.IndexOf(defaultMode);
                        cb.SelectedIndex = (index != -1) ? index : 0;
                    }

                    continue; // Skip CBDefault fallback
                }

                string configKey = $"CBDefault{cb.Name.Replace("comboBox", "")}";
                string defaultValue = ConfigurationManager.AppSettings[configKey];

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    int index = cb.Items.IndexOf(defaultValue);
                    cb.SelectedIndex = (index != -1) ? index : 0;
                }
            }

            if (neonForm != null)
                neonForm.IsInitializingDefaults = false;
        }


        private static IEnumerable<ComboBox> GetAllComboBoxes(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is ComboBox combo)
                    yield return combo;

                if (ctrl.HasChildren)
                {
                    foreach (ComboBox childCombo in GetAllComboBoxes(ctrl))
                        yield return childCombo;
                }
            }
        }
        /// <summary>
        /// Automatically binds all TextBox controls on the given form to their corresponding
        /// app.config settings using a naming convention. TextBoxes must be named with the
        /// prefix "txt" followed by the configuration key (e.g., txtExclusionTimeOffset).
        /// 
        /// On load, the method sets each TextBox’s text to its current config value.
        /// It also attaches a TextChanged event to update the config setting automatically
        /// whenever the user edits the TextBox.
        /// </summary>
        /// <param name="mainForm">The main form containing TextBox controls to bind.</param>
        public static void ApplyDefaultTextBoxSettings(Form mainForm)
        {
            if (mainForm == null) return;

            ConfigurationManager.RefreshSection("appSettings");

            foreach (TextBox tb in GetAllTextBoxes(mainForm))
            {
                if (tb.Name.StartsWith("txt"))
                {
                    string configKey = tb.Name.Replace("txt", "");

                    string value = ConfigurationManager.AppSettings[configKey];
                    tb.Text = value ?? "";

                    tb.TextChanged += (sender, e) =>
                    {
                        if (sender is TextBox changedTextBox)
                        {
                            string key = changedTextBox.Name.Replace("txt", "");
                            ConfigManager.SetAppSetting(key, changedTextBox.Text);
                            Logger.Info($"[TextBox Update] {key} = {changedTextBox.Text}");
                        }
                    };
                }
            }
        }

        private static IEnumerable<TextBox> GetAllTextBoxes(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is TextBox tb)
                    yield return tb;

                if (ctrl.HasChildren)
                {
                    foreach (var child in GetAllTextBoxes(ctrl))
                        yield return child;
                }
            }
        }
        /// <summary>
        /// Applies numeric-only formatting to predefined TextBoxes on the given form.
        /// Only TextBoxes whose names are in the internal list will be affected.
        /// </summary>
        /// <param name="mainForm">The form containing the TextBoxes.</param>
        public static void ApplyDefaultTextBoxNumberFormatting(Form mainForm)
        {
            if (mainForm == null) return;

            // Predefined list of numeric-only TextBoxes
            List<string> numericTextBoxNames = new List<string>
    {
        "txtExclusionTimeOffset",
        "txtVLCAudioReplayGain"
    };

            foreach (TextBox tb in GetAllTextBoxes(mainForm))
            {
                if (numericTextBoxNames.Contains(tb.Name))
                {
                    tb.KeyPress += (sender, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                            e.Handled = true;
                    };
                }
            }
        }
    }
}
