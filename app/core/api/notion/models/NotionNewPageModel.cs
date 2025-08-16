namespace theatredeck.app.core.api.notion.models
{
    public class NotionNewPageModel
    {
        public required List<string> ServerDrive { get; set; }
        public required string PlayNext { get; set; }
        public required int Year { get; set; }
        public required int Volume { get; set; }
        public required int PlayCount { get; set; }
        public required int StartTime { get; set; }
        public required string SkippingTime { get; set; }
        public required int EndTime { get; set; }
        public required string Location { get; set; }
        public required string Name { get; set; }
        public required List<string> Tags { get; set; }
        public required List<string> Collections { get; set; }
    }
}
