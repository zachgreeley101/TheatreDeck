using System.Windows.Forms;
using theatredeck.app.core.config;
using theatredeck.app.core.logger;
using theatredeck.app.features.vlc.utils;


namespace theatredeck.app.core.utils
{
    internal class RunOnStart
    {
        public static void Initialize(Form mainForm)
        {
            FileLogWriter.Initialize();
            ConfigManager.SetFormTextWithAppVersion(mainForm);
            NowNextStatusWriter.WriteStopToTimeFiles();
            UI_Helper.ApplyDefaultComboBoxSettings(mainForm);
            UI_Helper.ApplyDefaultTextBoxSettings(mainForm);
            UI_Helper.ApplyDefaultTextBoxNumberFormatting(mainForm);

        }
    }
}
