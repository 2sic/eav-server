using System.Text.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataWriter
{
    public string SerializeJsonBundle(JsonBundle bundleList, int indentation)
    {
        return System.Text.Json.JsonSerializer.Serialize(
            new JsonFormat
            {
                Bundles = [bundleList]
            },
            new JsonSerializerOptions(JsonOptions.UnsafeJsonWithoutEncodingHtml)
            {
                WriteIndented = indentation != 0
            }
        );
    }

}
