using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.ImportExport.Json
{
    public class JsonBundleSerializer: JsonSerializer
    {
        public JsonBundleSerializer(ITargetTypes metadataTargets) : base(metadataTargets, "Ser.Bundle") { }

        public string Serialize(BundleEntityWithAssets bundle, int metadataDepth)
        {
            // new in 11.07 - try to add assets
            var ent = ToJson(bundle.Entity, metadataDepth);
            if (bundle.Assets != null && bundle.Assets.Any()) ent.Assets = bundle.Assets;
            return JsonConvert.SerializeObject(new JsonFormat { Entity = ent }, JsonSettings.Defaults());
        }


        public new BundleEntityWithAssets Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false)
        {
            var package = UnpackAndTestGenericJsonV1(serialized);
            var entity = Deserialize(package.Entity, allowDynamic, skipUnknownType);
            var result = new BundleEntityWithAssets {Entity = entity, Assets = package.Entity.Assets};
            return result;
        }
    }
}
