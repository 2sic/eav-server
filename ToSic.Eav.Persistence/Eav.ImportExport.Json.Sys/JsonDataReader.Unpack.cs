using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataReader
{
    public JsonEntity UnpackEntityAndTestGenericJsonV1(string? serialized)
    {
        var format = UnpackAndTestGenericJsonV1(serialized);
        return format.Entity ?? throw new("No entity found in the package.");
    }

    public JsonFormat UnpackAndTestGenericJsonV1(string? serialized)
    {
        var l = LogDsDetails.Fn<JsonFormat>();

        if (string.IsNullOrWhiteSpace(serialized))
            throw LogException(new ArgumentNullException(nameof(serialized), @"cannot deserialize json - empty or null string"));

        JsonFormat jsonObj;
        try
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            jsonObj = System.Text.Json.JsonSerializer.Deserialize<JsonFormat>(serialized!, JsonOptions.UnsafeJsonWithoutEncodingHtml)!;
        }
        catch (Exception ex)
        {
            throw LogException(new FormatException("cannot deserialize json - bad format", ex));
        }

        if (jsonObj._.V != 1)
            throw LogException(new ArgumentOutOfRangeException(nameof(serialized), $@"unexpected format version: '{jsonObj._.V}'"));

        return l.Return(jsonObj);

        Exception LogException(Exception ex)
        {
            // In case of an error, do make sure that we do actually log it - even if the log-details was null
            var errLogger = l ?? Log.Fn<JsonFormat>();
            return errLogger.Done(ex);
        }
    }
}
