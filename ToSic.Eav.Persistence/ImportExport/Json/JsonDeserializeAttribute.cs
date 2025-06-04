using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.ImportExport.Json;

internal class JsonDeserializeAttribute
{
    internal static ContentTypeAttributeSysSettings SysSettings(string name, string serialized, ILog log)
    {
        // If nothing to process, exit early without logging
        if (serialized.IsEmpty())
            return null;

        var l = log.Fn<ContentTypeAttributeSysSettings>($"{name}: {serialized.Substring(0, Math.Min(50, serialized.Length))}...");

        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<JsonAttributeSysSettings>(serialized, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            return l.ReturnAsOk(json.ToSysSettings());
        }
        catch (Exception e)
        {
            l.Done(e);
            throw;
        }
    }

}