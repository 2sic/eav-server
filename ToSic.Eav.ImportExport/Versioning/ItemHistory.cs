using System;

namespace ToSic.Eav.ImportExport.Versioning
{
    public class ItemHistory
    {
        public DateTime TimeStamp { get; set; }
        public string User { get; set; }
        public int ChangeSetId { get; set; }
        public int HistoryId { get; set; }
        public int VersionNumber { get; set; }
        public string Data { get; set; }
    }

}