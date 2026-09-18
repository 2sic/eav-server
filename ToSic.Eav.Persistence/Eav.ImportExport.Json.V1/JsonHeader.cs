namespace ToSic.Eav.ImportExport.Json.V1;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record JsonHeader
{
    public int V { get; init; } = 1;
}