using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using theatredeck.app.features.scraper.models;
using theatredeck.app.features.scraper.utils;
using theatredeck.app.features.scraper.services;
using theatredeck.app.core.api.notion;
using theatredeck.app.core.api.notion.models;
using theatredeck.app.core.logger; // New Logger system

namespace theatredeck.app.features.scraper
{
    internal class ScraperController
    {
        private readonly ScraperConfig _config;
        private readonly NotionManager _notionManager;

        public ScraperController(ScraperConfig config, NotionManager notionManager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _notionManager = notionManager ?? throw new ArgumentNullException(nameof(notionManager));
        }

        public async Task<ScrapeJob> RunScrapeAsync(string driveName, Action<ScrapeJob> jobStatus = null)
        {
            var job = new ScrapeJob
            {
                ServerDrive = driveName,
                RootPath = _config.GetMediaDrivePath(driveName)
            };

            Logger.Info($"[START] Scraping started for {driveName} at {DateTime.Now} | Root: {job.RootPath}");

            if (string.IsNullOrWhiteSpace(job.RootPath))
            {
                job.StatusMessage = $"Error: Could not find path for drive '{driveName}'.";
                job.Errors.Add((driveName, job.StatusMessage));
                job.Complete();
                Logger.Error($"Could not find root path for {driveName}. Aborting scrape.");
                jobStatus?.Invoke(job);
                return job;
            }

            var folderPairs = FileScanner.GetMovieFoldersWithSingleMediaFile(job.RootPath);
            job.TotalFiles = folderPairs.Count;
            Logger.Info($"[SCAN] Found {folderPairs.Count} candidate folders in {job.RootPath}.");

            foreach (var (folder, mediaFile) in folderPairs)
            {
                Logger.Debug($"[PROCESS] Folder: {folder} | File: {mediaFile}");
                try
                {
                    var dbId = MediaParser.ExtractDatabaseId(mediaFile);
                    var year = MediaParser.ExtractYear(mediaFile) ?? MediaParser.ExtractYear(folder);
                    var title = MediaParser.ExtractTitle(mediaFile);
                    var ext = MediaParser.GetFileExtension(mediaFile);

                    var mediaInfo = new MediaFileInfo
                    {
                        FolderPath = folder,
                        MediaFilePath = mediaFile,
                        Title = title,
                        Year = year,
                        Extension = ext,
                        DatabaseId = dbId,
                        OriginalFileName = System.IO.Path.GetFileName(mediaFile)
                    };

                    if (mediaInfo.HasDatabaseId)
                    {
                        job.SkippedFiles.Add(mediaInfo);
                        job.FilesProcessed++;
                        job.StatusMessage = $"Skipped (already has DB ID): {mediaInfo.OriginalFileName}";
                        Logger.Info($"[SKIP] Already branded: {mediaInfo.OriginalFileName} in {folder}");
                        jobStatus?.Invoke(job);
                        continue;
                    }

                    Logger.Info($"[NOTION CREATE] Creating Notion page for: {mediaInfo.OriginalFileName} | Title: {mediaInfo.Title} | Year: {mediaInfo.Year} | Ext: {mediaInfo.Extension}");

                    var notionPage = new NotionNewPageModel
                    {
                        ServerDrive = new List<string> { driveName },
                        PlayNext = "false",
                        Year = mediaInfo.Year ?? 0,
                        Volume = 100,
                        PlayCount = 0,
                        StartTime = 0,
                        SkippingTime = "na",
                        EndTime = 8000,
                        Location = System.IO.Path.GetFileName(folder),
                        Name = mediaInfo.OriginalFileName,
                        Tags = new List<string> { ext, "Vol-Unset", "End-Unset", "Start-Unset" },
                        Collections = new List<string> { "NotUsed" }
                    };

                    string response = await _notionManager.CreateNotionPageAsync(notionPage);

                    var createdPage = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<theatredeck.app.core.api.notion.model.Page>(response);

                    var tempResponse = new theatredeck.app.core.api.notion.model.NotionResponse
                    {
                        Results = new List<theatredeck.app.core.api.notion.model.Page> { createdPage }
                    };
                    var models = _notionManager.ConvertResponseToModels(tempResponse);
                    var createdModel = models.FirstOrDefault();

                    string brandedId = createdModel?.IDSearch;

                    if (string.IsNullOrWhiteSpace(brandedId))
                        throw new Exception("Model returned an invalid or missing Search ID.");

                    Logger.Info($"[NOTION SUCCESS] Notion SearchID assigned: {brandedId}");

                    string newFilePath = FileRenameService.RenameFileWithDatabaseId(mediaFile, brandedId);
                    Logger.Info($"[RENAME] Renamed file: {mediaInfo.OriginalFileName} -> {System.IO.Path.GetFileName(newFilePath)}");

                    mediaInfo.DatabaseId = brandedId;
                    mediaInfo.MediaFilePath = newFilePath;
                    job.NewEntries.Add(mediaInfo);

                    job.StatusMessage = $"Added: {mediaInfo.OriginalFileName} → {System.IO.Path.GetFileName(newFilePath)}";
                    job.FilesProcessed++;
                    jobStatus?.Invoke(job);

                    Logger.Debug($"[DELAY] Waiting 10 seconds after processing {mediaInfo.OriginalFileName}");
                    await Task.Delay(10000);
                }
                catch (Exception ex)
                {
                    job.Errors.Add((mediaFile, ex.Message));
                    job.StatusMessage = $"Error: {System.IO.Path.GetFileName(mediaFile)} — {ex.Message}";
                    job.FilesProcessed++;
                    jobStatus?.Invoke(job);
                    Logger.Error($"[ERROR] File: {mediaFile} | Message: {ex.Message}", ex);
                }
            }

            job.StatusMessage = "Scraping complete.";
            job.Complete();
            jobStatus?.Invoke(job);

            Logger.Info($"[COMPLETE] Scraping complete for {driveName}. Added: {job.NewEntries.Count}, Skipped: {job.SkippedFiles.Count}, Errors: {job.Errors.Count}. Time: {job.Duration}");

            return job;
        }
    }
}
