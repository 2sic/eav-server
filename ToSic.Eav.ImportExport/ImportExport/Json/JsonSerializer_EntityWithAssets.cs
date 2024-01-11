using System.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

partial class JsonSerializer
{
    public string Serialize(BundleEntityWithAssets bundle, int metadataDepth)
    {
        var l = Log.Fn<string>($"metadataDepth:{metadataDepth}");
        // new in 11.07 - try to add assets
        var ent = ToJson(bundle.Entity, metadataDepth);
        if (bundle.Assets != null && bundle.Assets.Any()) ent.Assets = bundle.Assets;
        return l.ReturnAsOk(System.Text.Json.JsonSerializer.Serialize(new JsonFormat { Entity = ent }, JsonOptions.UnsafeJsonWithoutEncodingHtml));
    }
}