using System;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Case insensitive dynamic read-object for JSON. <br/>
    /// Used in various cases where you start with JSON and want to provide the contents to custom code without having to mess with
    /// JS/C# code style differences. 
    /// </summary>
    [PrivateApi]
    public /*partial*/ class DynamicJacket: DynamicObject
    {
        public readonly JToken OriginalData;

        public const string EmptyJson = "{}";
        private const char JObjStart = '{';
        private const char JArrayStart = '[';
        private const string JsonErrorCode = "error";


        public DynamicJacket(JObject originalData) => OriginalData = originalData;

        //public DynamicJacket(string json, string fallback = EmptyJson) => OriginalData = AsDynamic(json, fallback);

        public static object AsDynamicJacket(string json, string fallback = EmptyJson)
        {
            return WrapOrUnwrap(AsDynamic(json, fallback));
            //var tbd = AsDynamic(json, fallback);
            //if(tbd is JObject jObject)
            //    return new DynamicJacket(jObject);

            //if(tbd is JArray jArray)
            //    return new DynamicJacketList(jArray);

            //return tbd;
        }

        private static JToken AsDynamic(string json, string fallback = EmptyJson)
        {
            if (!string.IsNullOrWhiteSpace(json))
                try
                {
                    // find first possible opening character
                    var firstCharPos = json.IndexOfAny(new[] { JObjStart, JArrayStart });
                    if (firstCharPos > -1)
                    {
                        var firstChar = json[firstCharPos];
                        if (firstChar == JObjStart)
                            return JObject.Parse(json);
                        if (firstChar == JArrayStart)
                            return JArray.Parse(json);
                    }
                }
                catch
                {
                    if (fallback == JsonErrorCode) throw;
                }

            // fallback
            return fallback == null
                ? null
                : JObject.Parse(fallback);
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!OriginalData.HasValues)
            {
                result = null;
                return true;
            }

            if (OriginalData is JObject jData)
            {
                var found = jData.Properties()
                    .FirstOrDefault(
                        p => string.Equals(p.Name, binder.Name, StringComparison.InvariantCultureIgnoreCase));

                if (found != null)
                {
                    var original = found.Value;
                    result = WrapOrUnwrap(original);
                    return true;
                }
            }

            // not found
            result = null;
            return true;
        }

        public static object WrapOrUnwrap(object original)
        {
            switch (original)
            {
                case JArray jArray:
                    return new DynamicJacketList(jArray); //  jArray.Select(WrapOrUnwrap).ToArray();
                case JObject jResult: // it's another complex object, so return another wrapped reader
                    return new DynamicJacket(jResult);
                case JValue jValue: // it's a simple value - so we want to return the underlying real value
                    return jValue.Value;
                default: // it's something else, let's just return that
                    return original;
            }
        }

    }
}
