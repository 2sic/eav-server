using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.ImportExport.Json;

internal class JsonDeserializeAttribute
{
    internal static ContentTypeAttributeSysSettings SysSettings(string serialized, ILog log)
    {
        // If nothing to process, exit early without logging
        if (serialized.IsEmpty())
            return null;

        var l = log.Fn<ContentTypeAttributeSysSettings>($"{serialized.Substring(0, Math.Min(50, serialized.Length))}...");

        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<JsonAttributeSysSettings>(serialized, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            return l.Return(json.ToSysSettings(), "deserialized sysSettings");
        }
        catch (Exception e)
        {
            l.Done(e);
            throw;
        }
    }

}