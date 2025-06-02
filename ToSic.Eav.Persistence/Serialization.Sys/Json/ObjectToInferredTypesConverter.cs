using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ToSic.Eav.Serialization;

/// <summary>
/// Deserialize inferred types to .NET types
///
/// When deserializing to a property of type object, a JsonElement object is created.
/// The reason is that the deserializer doesn't know what CLR type to create, and it doesn't try to guess.
/// For example, if a JSON property has "true", the deserializer doesn't infer that the value is a Boolean,
/// and if an element has "01/01/2019", the deserializer doesn't infer that it's a DateTime.
/// 
/// Type inference can be inaccurate. If the deserializer parses a JSON number that has no decimal point as a long,
/// that might result in out-of-range issues if the value was originally serialized as a ulong or BigInteger.
/// Parsing a number that has a decimal point as a double might lose precision if the number was originally serialized as a decimal.
/// 
/// For scenarios that require type inference, we are doing conversions:
/// - true and false to Boolean
/// - Numbers without a decimal to int or long
/// - Numbers with a decimal to double
/// - Strings like Guids to Guid
/// - Dates to DateTime
/// - Strings to string
/// - Null to null
/// - Object to JsonObject
/// - Array to JsonArray
/// - Everything else to JsonElement
///  </summary>
/// <param name="original"></param>
/// <returns></returns>
/// <remarks>
/// More info:
/// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#deserialize-inferred-types-to-object-properties
/// https://stackoverflow.com/questions/65972825/c-sharp-deserializing-nested-json-to-nested-dictionarystring-object/65974452#65974452
/// https://stackoverflow.com/questions/68519985/how-do-i-get-system-text-json-to-deserialize-objects-into-their-original-type
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number when reader.TryGetInt32(out var intValue):
                return intValue;
            case JsonTokenType.Number when reader.TryGetInt64(out var longValue):
                return longValue;
            case JsonTokenType.Number:
                return reader.GetDouble();

            // 2022-10-17 2dm/STV disabled this again, because Newtonsoft never auto-converted GUIDs and it's causing side effects.
            //case JsonTokenType.String when reader.TryGetGuid(out var guidValue):
            //    return guidValue;
            case JsonTokenType.String when reader.TryGetDateTime(out var dateTime):
                return dateTime;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartObject:
                return JsonObject.Create(JsonDocument.ParseValue(ref reader).RootElement.Clone());
            case JsonTokenType.StartArray:
                return JsonArray.Create(JsonDocument.ParseValue(ref reader).RootElement.Clone());
            default:
                var x = JsonDocument.ParseValue(ref reader).RootElement.Clone();
                return x;
        }
    }

    protected virtual IDictionary<string, object> CreateDictionary() => new ExpandoObject();

    public override void Write(
        Utf8JsonWriter writer,
        object objectToWrite,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
}