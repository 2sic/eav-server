using System;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public partial class ToSicEavDataTimeline
    {
        public int Id { get; set; }
        public string SourceTable { get; set; }
        public int? SourceId { get; set; }
        public Guid? SourceGuid { get; set; }
        public string SourceTextKey { get; set; }
        public string Operation { get; set; }
        public DateTime SysCreatedDate { get; set; }
        public int? SysLogId { get; set; }
        public string NewData { get; set; }
        public string Json { get; set; }
    }
}
