using System.Collections.Generic;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public interface IJsonWithAssets
    {
        List<JsonAsset> Assets { get; set; }
    }
}
