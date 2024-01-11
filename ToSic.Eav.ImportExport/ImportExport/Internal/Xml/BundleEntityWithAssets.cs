using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Internal.Xml;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class BundleEntityWithAssets
{
    public IEntity Entity;
    public List<JsonAsset> Assets = [];
}