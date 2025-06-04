using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Sys.Xml;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class BundleEntityWithAssets
{
    public IEntity Entity;
    public List<JsonAsset> Assets = [];
}