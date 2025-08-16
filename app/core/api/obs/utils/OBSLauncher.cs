using Microsoft.VisualBasic.Devices;
using theatredeck.app.forms;
using theatredeck.Properties;
using theatredeck.app.core.logger;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace theatredeck.app.core.api.obs.utils
{
    internal static class OBSLauncher
    {
        // Constants for executable details
        private const string OBSExecutable = "obs64.exe";
        private const string OBSArguments = "--portable";

        internal static void LaunchOBS(string obsType)
        {
            try
            {
                string obsLibraryPath = ConfigurationManager.AppSettings["OBSLibraryPath"];
                if (string.IsNullOrWhiteSpace(obsLibraryPath))
                    throw new InvalidOperationException("OBSLibraryPath is not configured in app.config.");

                const string OBSScreenSubDirectory = @"screen\obs-studio\bin\64bit";
                const string OBSCameraSubDirectory = @"camera\obs-studio\bin\64bit";

                string obsSubDirectory = obsType.Equals("Screen", StringComparison.OrdinalIgnoreCase)
                    ? OBSScreenSubDirectory
                    : obsType.Equals("Camera", StringComparison.OrdinalIgnoreCase)
                        ? OBSCameraSubDirectory
                        : throw new ArgumentException("Invalid OBS type. Use \"Screen\" or \"Camera\".", nameof(obsType));

                string obsWorkingDirectory = Path.Combine(obsLibraryPath, obsSubDirectory);
                string obsExePath = Path.Combine(obsWorkingDirectory, OBSExecutable);

                if (!File.Exists(obsExePath))
                {
                    Logger.Error($"OBS executable not found at: {obsExePath}");
                    return;
                }

                Logger.Info($"Launching OBS ({obsType}) from: {obsExePath}");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = obsExePath,
                    WorkingDirectory = obsWorkingDirectory,
                    Arguments = OBSArguments
                };

                Process.Start(startInfo);

                // After launching OBS, start auto-connecting to the relevant WebSocket
                TheatreDeckForm.Instance._obsManager.StartAutoConnect(obsType);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to launch OBS ({obsType}): {ex.Message}", ex);
            }
        }
    }
}
