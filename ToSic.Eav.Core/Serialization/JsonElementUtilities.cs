using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ToSic.Eav.Serialization
{
    public static class JsonElementUtilities
    {
        public static Dictionary<string, object> UnwrapJsonElementsInDictionary<T>(T dictionary) where T : IDictionary<string, object>
        {
            var unwrappedDictionary = new Dictionary<string, object>();
            foreach (var pair in dictionary)
                unwrappedDictionary[pair.Key] = UnwrapJsonElement(pair.Value);
            return unwrappedDictionary;
        }

        public static object UnwrapJsonElement(object original)
        {
            if (!(original is JsonElement jsonElement)) return original;

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    if (jsonElement.TryGetGuid(out var guidValue))
                        return guidValue;
                    if (jsonElement.TryGetDateTime(out var dateTime))
                    {
                        //if (dateTime.Kind == DateTimeKind.Local)
                        //    if (jsonElement.TryGetDateTimeOffset(out var datetimeOffset))
                        //        return datetimeOffset;
                        return dateTime;
                    }
                    return jsonElement.ToString();
                case JsonValueKind.Number:
                    if (jsonElement.TryGetInt32(out var intValue))
                        return intValue;
                    if (jsonElement.TryGetInt64(out var longValue))
                        return longValue;
                    return jsonElement.GetDouble();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean();
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    return JsonObject.Create(jsonElement);
                    //dynamic unwrappedObject = new ExpandoObject();
                    //foreach (var property in jsonElement.EnumerateObject())
                    //    ((IDictionary<string, object>)unwrappedObject)[property.Name] = UnwrapJsonElement(property.Value);
                    //return unwrappedObject;
                case JsonValueKind.Array:
                    return JsonArray.Create(jsonElement);
                    //var unwrappedArray = new ArrayList();
                    //foreach (var item in jsonElement.EnumerateArray())
                    //    unwrappedArray.Add(UnwrapJsonElement(item));
                    //return unwrappedArray.ToArray();
                case JsonValueKind.Undefined:
                default:
                    return jsonElement;
            }
        }

        public static Type ToType(this JsonValueKind valueKind, string value)
        {
            switch (valueKind)
            {
                case JsonValueKind.String:
                    return Guid.TryParse(value, out var guidValue) ? typeof(Guid) : 
                        DateTime.TryParse(value, out var dateTimeValue) ? typeof(DateTime) : typeof(string);
                case JsonValueKind.Number:
                    return int.TryParse(value, out var intValue) ?  typeof(int) : 
                        long.TryParse(value, out var longValue) ? typeof(long) : typeof(double);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return typeof(bool);
                case JsonValueKind.Array:
                    return typeof(Array);
                case JsonValueKind.Undefined:
                    throw new NotSupportedException();
                case JsonValueKind.Null:
                case JsonValueKind.Object:
                default:
                    return typeof(object);
            }
        }
    }
}
