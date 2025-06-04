using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{
    public BundleEntityWithAssets DeserializeEntityWithAssets(string serialized, bool allowDynamic = false, bool skipUnknownType = false)
    {
        var l = LogDsDetails.Fn<BundleEntityWithAssets>();
        var package = UnpackAndTestGenericJsonV1(serialized);
        var entity = Deserialize(package.Entity, allowDynamic, skipUnknownType);
        var result = new BundleEntityWithAssets { Entity = entity, Assets = package.Entity.Assets };
        return l.ReturnAsOk(result);
    }
}