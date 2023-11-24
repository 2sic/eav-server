using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class BundleEntityWithAssets
{
    public IEntity Entity;
    public List<JsonAsset> Assets = new();
}