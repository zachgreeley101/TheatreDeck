using System;
using System.Collections.Generic;

namespace theatredeck.app.core.api.notion.models
{
    public class NotionDatabaseModel
    {
        public string PageId { get; set; }
        public string IDSearch { get; set; }
        public List<string> ServerDrive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool PlayNext { get; set; }
        public int? Year { get; set; }
        public int? Volume { get; set; }
        public int? PlayCount { get; set; }
        public int? StartTime { get; set; }
        public string SkippingTime { get; set; }
        public int? EndTime { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Location { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Collections { get; set; }

        public NotionDatabaseModel(
            string pageId,
            string idSearch,
            List<string> serverDrive,
            DateTime? createdDate,
            bool playNext,
            int? year,
            int? volume,
            int? playCount,
            int? startTime,
            string skippingTime,
            int? endTime,
            DateTime? modifiedDate,
            string location,
            string id,
            string name,
            List<string> tags,
            List<string> collections
        )
        {
            PageId = pageId;
            IDSearch = idSearch;
            ServerDrive = serverDrive;
            CreatedDate = createdDate;
            PlayNext = playNext;
            Year = year;
            Volume = volume;
            PlayCount = playCount;
            StartTime = startTime;
            SkippingTime = skippingTime;
            EndTime = endTime;
            ModifiedDate = modifiedDate;
            Location = location;
            ID = id;
            Name = name;
            Tags = tags;
            Collections = collections;
        }

        public NotionDatabaseModel()
        {
            ServerDrive = new List<string>();
            Tags = new List<string>();
            Collections = new List<string>();
        }
    }
}
