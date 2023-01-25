using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.Serialization;


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

        public List<IContentType> BundleContentTypes(JsonFormat package)
        {
            var result = new List<IContentType>();

            if (package.Bundles?.Any() != true) return result;

            foreach (var bundle in package.Bundles)
                // convert JsonContentTypeSet to ContentTypeSet
                if (bundle.ContentTypes?.Any() == true)
                    foreach (var jsonContentTypeSet in bundle.ContentTypes)
                        result.Add(ConvertContentType(jsonContentTypeSet));

            return result;
        }

        public List<IEntity> BundleEntities(JsonFormat package, IEntitiesSource relationshipSource = null)
        {
            var result = new List<IEntity>();

            if (package.Bundles?.Any() != true) return result;
            
            foreach (var bundle in package.Bundles)
                if (bundle.Entities?.Any() == true)
                    // convert JsonEntities to Entities
                    result.AddRange(Deserialize(bundle.Entities, allowDynamic: true, skipUnknownType: false,
                        relationshipSource));

            return result;
        }

        private List<IEntity> Deserialize(IEnumerable<JsonEntity> entities, bool allowDynamic, bool skipUnknownType, IEntitiesSource dynRelationshipsSource = null) =>
            entities.Select(entity => Deserialize(entity, allowDynamic, skipUnknownType, dynRelationshipsSource)).ToList();
    }
}
