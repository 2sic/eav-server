using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.ImportExport.Json.Sys;

public class JsonDeserializeAttribute
{
    /// <summary>
    /// De-serialize ContentTypeAttributeSysSettings from SysSettings string field in ToSicEavAttributes and Content-Types (EF/DB)
    /// </summary>
    /// <returns>ContentTypeAttributeSysSettings or null</returns>
    public static ContentTypeAttributeSysSettings? SysSettings(string? nameForLog, string? serialized, ILog? logOrNull)
    {
        // If nothing to process, exit early without logging
        if (serialized.IsEmpty())
            return null;

        var l = logOrNull.Fn<ContentTypeAttributeSysSettings?>($"{nameForLog}: {serialized.Substring(0, Math.Min(50, serialized.Length))}...");

        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<JsonAttributeSysSettings>(serialized, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            return l.ReturnAsOk(json?.ToSysSettings());
        }
        catch (Exception e)
        {
            l.Done(e);
            throw;
        }
    }

}