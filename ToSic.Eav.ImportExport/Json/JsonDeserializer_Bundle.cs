using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public string SerializeJsonBundle(JsonBundle bundleList) =>
            System.Text.Json.JsonSerializer.Serialize(new JsonFormat { Bundles = new List<JsonBundle>() { bundleList } }, JsonOptions.UnsafeJsonWithoutEncodingHtml);

        internal List<IContentType> GetContentTypesFromBundles(JsonFormat package) => Log.Func(() =>
        {
            if (package.Bundles?.Any() != true) return new List<IContentType>();

            var result = package.Bundles
                .Where(bundle => bundle.ContentTypes?.Any() == true)
                .SelectMany(bundle => bundle.ContentTypes.Select(ConvertContentType))
                .ToList();

            return result;
        });

        internal List<IEntity> GetEntitiesFromBundles(JsonFormat package, IEntitiesSource relationshipSource = null) => Log.Func(() =>
        {
            if (package.Bundles?.Any() != true) return new List<IEntity>();

            var result = package.Bundles
                .Where(bundle => bundle.Entities?.Any() == true)
                .SelectMany(bundle => bundle.Entities.Select(e => Deserialize(e, true, false, relationshipSource)))
                .ToList();

            return result;
        });
    }
}
