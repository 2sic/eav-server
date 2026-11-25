using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Persistence.Versions;

[ContentType(
    Name = "ItemHistory",
    Guid = "3c5b1980-b1f6-4913-bdef-36113f5da2da")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record ItemHistory : IRawEntityAutoConvert
{
    public DateTime TimeStamp { get; init; }
    public string? User { get; init; }
    public int ChangeSetId { get; init; }
    public int HistoryId { get; init; }
    public int VersionNumber { get; init; }
    public string? Json { get; init; }

}