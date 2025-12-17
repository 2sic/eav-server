// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

// https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class TsDynDataHistory
{
    public int HistoryId { get; set; }

    public required string SourceTable { get; set; }

    public int? SourceId { get; set; }

    public Guid? SourceGuid { get; set; }

    public required string Operation { get; set; }

    public DateTime Timestamp { get; set; }

    public int? TransactionId { get; set; }

    public string? ParentRef { get; set; }

    public string? Json { get; set; }

    public byte[]? CJson { get; set; }
    public virtual TsDynDataTransaction Transaction { get; set; } = null!;
}