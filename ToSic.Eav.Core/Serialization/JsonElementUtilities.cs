using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ToSic.Eav.Serialization
{
    public static class JsonElementUtilities
    {
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
        //public static object UnwrapJsonElement(object original)
        //{
        //    if (!(original is JsonElement jsonElement)) return original;

        //    switch (jsonElement.ValueKind)
        //    {
        //        case JsonValueKind.True:
        //            return true;
        //        case JsonValueKind.False:
        //            return false;
        //        case JsonValueKind.Number when jsonElement.TryGetInt32(out var intValue):
        //            return intValue;
        //        case JsonValueKind.Number when jsonElement.TryGetInt64(out var longValue):
        //            return longValue;
        //        case JsonValueKind.Number:
        //            return jsonElement.GetDouble();
        //        case JsonValueKind.String when jsonElement.TryGetGuid(out var guidValue):
        //                return guidValue;
        //        case JsonValueKind.String when jsonElement.TryGetDateTime(out var dateTime):
        //            {
        //                //if (dateTime.Kind == DateTimeKind.Local)
        //                //    if (jsonElement.TryGetDateTimeOffset(out var datetimeOffset))
        //                //        return datetimeOffset;
        //                return dateTime;
        //            }
        //        case JsonValueKind.String:
        //            return jsonElement.ToString();
        //        case JsonValueKind.Null:
        //            return null;
        //        case JsonValueKind.Object:
        //            return JsonObject.Create(jsonElement);
        //        case JsonValueKind.Array:
        //            return JsonArray.Create(jsonElement);
        //        case JsonValueKind.Undefined:
        //        default:
        //            return jsonElement;
        //    }
        //}

        //public static Dictionary<string, object> UnwrapJsonElementsInDictionary<T>(T dictionary) where T : IDictionary<string, object>
        //{
        //    var unwrappedDictionary = new Dictionary<string, object>();
        //    foreach (var pair in dictionary)
        //        unwrappedDictionary[pair.Key] = UnwrapJsonElement(pair.Value);
        //    return unwrappedDictionary;
        //}

        //public static Type ToType(this JsonValueKind valueKind, string value)
        //{
        //    switch (valueKind)
        //    {
        //        case JsonValueKind.String:
        //            return Guid.TryParse(value, out var guidValue) ? typeof(Guid) :
        //                DateTime.TryParse(value, out var dateTimeValue) ? typeof(DateTime) : typeof(string);
        //        case JsonValueKind.Number:
        //            return int.TryParse(value, out var intValue) ? typeof(int) :
        //                long.TryParse(value, out var longValue) ? typeof(long) : typeof(double);
        //        case JsonValueKind.True:
        //        case JsonValueKind.False:
        //            return typeof(bool);
        //        case JsonValueKind.Array:
        //            return typeof(Array);
        //        case JsonValueKind.Undefined:
        //            throw new NotSupportedException();
        //        case JsonValueKind.Null:
        //        case JsonValueKind.Object:
        //        default:
        //            return typeof(object);
        //    }
        //}
    }
}
