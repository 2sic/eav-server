using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.Metadata;
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

        public BundleEntityWithAssets DeserializeBundle(string serialized, bool allowDynamic = false, bool skipUnknownType = false)
        {
            var package = UnpackAndTestGenericJsonV1(serialized);
            var entity = Deserialize(package.Entity, allowDynamic, skipUnknownType);
            var result = new BundleEntityWithAssets { Entity = entity, Assets = package.Entity.Assets };
            return result;
        }

        public string SerializeJsonBundle(JsonBundle bundleList) =>
            System.Text.Json.JsonSerializer.Serialize(new JsonFormat { Bundles = new List<JsonBundle>() { bundleList } }, JsonOptions.UnsafeJsonWithoutEncodingHtml);

        public List<Bundle> DeserializeContentTypes(string serialized)
        {
            var package = UnpackAndTestGenericJsonV1(serialized);
            var result = new List<Bundle>();

            if (package.Bundles?.Any() == true)
            {
                foreach (var bundle in package.Bundles)
                {
                    var bundleResult = new Bundle();
                    result.Add(bundleResult);

                    if (bundle.ContentTypes?.Any() == true)
                    {
                        // convert JsonContentTypeSet to ContentTypeSet
                        foreach (var jsonContentTypeSet in bundle.ContentTypes)
                        {
                            if (bundleResult.ContentTypeSets == null) bundleResult.ContentTypeSets = new List<ContentTypeSet>();

                            var contentTypeSet = new ContentTypeSet();
                            bundleResult.ContentTypeSets.Add(contentTypeSet);

                            // convert JsonContentType to ContentType
                            contentTypeSet.ContentType = ConvertContentType(jsonContentTypeSet);
                        }
                    }

                }
            }

            return result;
        }

        public List<Bundle> DeserializeEntities(string serialized, int id = 0, IEntitiesSource relationshipSource = null)
        {
            var package = UnpackAndTestGenericJsonV1(serialized);
            var result = new List<Bundle>();

            if (package.Bundles?.Any() == true)
            {
                foreach (var bundle in package.Bundles)
                {
                    var bundleResult = new Bundle();
                    result.Add(bundleResult);

                    if (bundle.Entities?.Any() == true)
                    {
                        if (id != 0) bundle.Entities.ForEach(je => je.Id = ++id);

                        // convert JsonEntities to Entities
                        bundleResult.Entities = Deserialize(bundle.Entities, allowDynamic: true, skipUnknownType: false, relationshipSource);

                        if (id == 0) bundleResult.Entities.ForEach(e => e.ResetEntityId());
                    }
                }
            }

            return result;
        }

        private List<Entity> Deserialize(IEnumerable<JsonEntity> entities, bool allowDynamic, bool skipUnknownType, IEntitiesSource dynRelationshipsSource = null) =>
            entities.Select(entity => (Entity)Deserialize(entity, allowDynamic, skipUnknownType, dynRelationshipsSource)).ToList();
    }
}
