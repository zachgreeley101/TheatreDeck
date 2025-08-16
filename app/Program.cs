using theatredeck.app.core.utils;
using theatredeck.app.forms;
using System;
using System.Windows.Forms;

namespace theatredeck.src
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();

                // Create the main form.
                var mainForm = new TheatreDeckForm();

                // Initialize logging, settings, and set form text.
                RunOnStart.Initialize(mainForm);

                // Run the application.
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while starting the application: {ex.Message}",
                                "Application Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}