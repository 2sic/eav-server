﻿// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
#nullable disable // This is EFC code; values will be auto-generated on compile

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataHistory
{
    public int HistoryId { get; set; }

    public string SourceTable { get; set; }

    public int? SourceId { get; set; }

    public Guid? SourceGuid { get; set; }

    public string Operation { get; set; }

    public DateTime Timestamp { get; set; }

    public int? TransactionId { get; set; }

    public string Json { get; set; }

    public byte[] CJson { get; set; }
    public virtual TsDynDataTransaction Transaction { get; set; }
}