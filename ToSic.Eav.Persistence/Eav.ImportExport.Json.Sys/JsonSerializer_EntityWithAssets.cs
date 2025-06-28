using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{
    public string Serialize(BundleEntityWithAssets bundle, int metadataDepth)
    {
        var l = Log.Fn<string>($"metadataDepth:{metadataDepth}");
        // new in 11.07 - try to add assets
        var ent = ToJson(bundle.Entity, metadataDepth);
        if (bundle.Assets is { Count: > 0 })
            ent = ent with { Assets = bundle.Assets };
        return l.ReturnAsOk(System.Text.Json.JsonSerializer.Serialize(new JsonFormat { Entity = ent }, JsonOptions.UnsafeJsonWithoutEncodingHtml));
    }
}