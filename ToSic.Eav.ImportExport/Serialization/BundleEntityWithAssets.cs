using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Serialization
{
    public class BundleEntityWithAssets
    {
        public IEntity Entity;
        public List<JsonAsset> Assets = new List<JsonAsset>();
    }
}
