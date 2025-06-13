using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{
    public BundleEntityWithAssets DeserializeEntityWithAssets(string serialized, bool allowDynamic = false, bool skipUnknownType = false)
    {
        var l = LogDsDetails.Fn<BundleEntityWithAssets>();
        var package = UnpackAndTestGenericJsonV1(serialized);
        var jsonEntity = package.Entity ?? throw new("No entity found in the package.");
        var entity = Deserialize(jsonEntity, allowDynamic, skipUnknownType);
        var result = new BundleEntityWithAssets
        {
            Entity = entity,
            Assets = jsonEntity.Assets ?? []
        };
        return l.ReturnAsOk(result);
    }
}