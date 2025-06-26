using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Sys.Xml;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class BundleEntityWithAssets
{
    public required IEntity Entity { get; init; }
    public ICollection<JsonAsset> Assets { get; init; }= [];
}