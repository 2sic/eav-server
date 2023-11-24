using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

partial class JsonSerializer
{
    public BundleEntityWithAssets DeserializeEntityWithAssets(string serialized, bool allowDynamic = false, bool skipUnknownType = false)
    {
        var l = Log.Fn<BundleEntityWithAssets>();
        var package = UnpackAndTestGenericJsonV1(serialized);
        var entity = Deserialize(package.Entity, allowDynamic, skipUnknownType);
        var result = new BundleEntityWithAssets { Entity = entity, Assets = package.Entity.Assets };
        return l.ReturnAsOk(result);
    }
}