using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public string Serialize(BundleEntityWithAssets bundle, int metadataDepth)
        {
            // new in 11.07 - try to add assets
            var ent = ToJson(bundle.Entity, metadataDepth);
            if (bundle.Assets != null && bundle.Assets.Any()) ent.Assets = bundle.Assets;
            return System.Text.Json.JsonSerializer.Serialize(new JsonFormat { Entity = ent }, JsonOptions.UnsafeJsonWithoutEncodingHtml);
        }

        public BundleEntityWithAssets DeserializeEntityWithAssets(string serialized, bool allowDynamic = false, bool skipUnknownType = false)
        {
            var package = UnpackAndTestGenericJsonV1(serialized);
            var entity = Deserialize(package.Entity, allowDynamic, skipUnknownType);
            var result = new BundleEntityWithAssets { Entity = entity, Assets = package.Entity.Assets };
            return result;
        }

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
