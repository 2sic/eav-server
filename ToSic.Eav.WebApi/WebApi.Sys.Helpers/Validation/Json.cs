using System.Text.Json;
using System.Text.Json.Nodes;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.WebApi.Sys.Helpers.Validation;

internal class Json
{
    public static bool IsValidJson(string strInput)
    {
        strInput = strInput.Trim();
        if (!(strInput.StartsWith("{") && strInput.EndsWith("}")) &&
            !(strInput.StartsWith("[") && strInput.EndsWith("]")))
            // it is not js Object and not js Array
            return false;

        try
        {
            JsonNode.Parse(strInput, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions);
        }
        catch (JsonException)
        {
            //  exception in parsing json
            return false;
        }
        catch (Exception)
        {
            // some other exception
            return false;
        }

        // json is valid
        return true;
    }
}