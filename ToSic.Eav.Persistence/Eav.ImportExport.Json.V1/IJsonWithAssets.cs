namespace ToSic.Eav.ImportExport.Json.V1;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IJsonWithAssets
{
    ICollection<JsonAsset>? Assets { get; init; }
}