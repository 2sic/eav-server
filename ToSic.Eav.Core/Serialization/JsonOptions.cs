using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ToSic.Eav.Serialization
{
    public static class JsonOptions
    {
        public const int DefaultMaxModelBindingRecursionDepth = 32;


        /// <summary>
        /// Common, shared, default, base JsonSerializerOptions for JsonSerializer
        /// provided to all other concrete JsonSerializerOptions implementations 
        /// also to preserve json compatibility with older apis used by Newtonsoft.Json.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            Converters = { new JsonDateTimeConverter(), new JsonStringEnumConverter() },
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IncludeFields = true,
            // Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
            // from deserialization errors that might occur from deeply nested objects.
            // This value is the same for model binding and Json.Net's serialization.
            MaxDepth = DefaultMaxModelBindingRecursionDepth,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null, // leave property names unchanged (PascalCase for c#)
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
        };

        /// <summary>
        /// Most used set of JsonSerializerOptions for use in every JsonSerializer.
        /// Provided as default set of options for SystemTextJsonMediaTypeFormatter used by our apis in DNN.
        /// Provided as default set of options for SystemTextJsonOutputFormatter used by apis in Oqtane.
        /// Compared to the default encoder, the UnsafeRelaxedJsonEscaping encoder is more permissive about allowing characters to pass through unescaped:
        /// - It doesn't escape HTML-sensitive characters such as <, >, &, and '.
        /// - It doesn't offer any additional defense-in-depth protections against XSS or information disclosure attacks, such as those which might result from the client and server disagreeing on the charset.
        /// Use the unsafe encoder only when it's known that the client will be interpreting the resulting payload as UTF-8 encoded JSON.
        /// For example, you can use it if the server is sending the response header Content-Type: application/json; charset=utf-8.
        /// Never allow the raw UnsafeRelaxedJsonEscaping output to be emitted into an HTML page or a <script> element.
        /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-encoding#serialize-all-characters
        /// </summary>
        public static JsonSerializerOptions UnsafeJsonWithoutEncodingHtml = new JsonSerializerOptions(DefaultOptions)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// When json is used in html attributes, everything except basic charters should be encoded:
        /// - escape HTML-sensitive characters such as <, >, &, and '.
        /// - additional defense-in-depth protections against XSS or information disclosure attacks, such as those which might result from the client and server disagreeing on the charset.
        /// This is alternative to UnsafeJsonWithoutEncodingHtml that is using UnsafeRelaxedJsonEscaping encoder.
        /// </summary>
        public static JsonSerializerOptions SafeJsonForHtmlAttributes = new JsonSerializerOptions(DefaultOptions)
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CurrencySymbols/*, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB, UnicodeRanges.LatinExtendedC, UnicodeRanges.LatinExtendedD, UnicodeRanges.LatinExtendedE, UnicodeRanges.LatinExtendedAdditional*/),
        };

        /// <summary>
        /// For use in features.json to preserve simpler datetime format.
        /// </summary>
        public static JsonSerializerOptions FeaturesJson = new JsonSerializerOptions(DefaultOptions)
        {
            Converters = { new JsonShortDateTimeConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>
        /// Default JsonDocumentOptions for direct use with JsonNode.Parse and JsonDocument.Parse
        /// to preserve json compatibility with older apis used by Newtonsoft.Json.
        /// </summary>
        public static JsonDocumentOptions JsonDocumentDefaultOptions = new JsonDocumentOptions()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
            // Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
            // from deserialization errors that might occur from deeply nested objects.
            // This value is the same for model binding and Json.Net's serialization.
            MaxDepth = DefaultMaxModelBindingRecursionDepth,
        };

        /// <summary>
        /// Default JsonNodeOptions for direct use with JsonNode.Parse
        /// to preserve json compatibility with older apis used by Newtonsoft.Json.
        /// </summary>
        public static JsonNodeOptions JsonNodeDefaultOptions = new JsonNodeOptions()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Used to set default set of options for SystemTextJsonInputFormatter used by apis in Oqtane
        /// also to preserve json compatibility with older apis used by Newtonsoft.Json.
        /// Compared to the default encoder, the UnsafeRelaxedJsonEscaping encoder is more permissive about allowing characters to pass through unescaped:
        /// - It doesn't escape HTML-sensitive characters such as <, >, &, and '.
        /// - It doesn't offer any additional defense-in-depth protections against XSS or information disclosure attacks, such as those which might result from the client and server disagreeing on the charset.
        /// Use the unsafe encoder only when it's known that the client will be interpreting the resulting payload as UTF-8 encoded JSON.
        /// For example, you can use it if the server is sending the response header Content-Type: application/json; charset=utf-8.
        /// Never allow the raw UnsafeRelaxedJsonEscaping output to be emitted into an HTML page or a <script> element.
        /// </summary>
        /// <param name="value"></param>
        public static void SetUnsafeJsonSerializerOptions(this JsonSerializerOptions value)
        {
            value.AllowTrailingCommas = true;
            value.Converters.Add(new JsonDateTimeConverter()); 
            value.Converters.Add(new JsonStringEnumConverter());
            //value.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            value.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            value.IncludeFields = true;
            // Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
            // from deserialization errors that might occur from deeply nested objects.
            // This value is the same for model binding and Json.Net's serialization.
            value.MaxDepth = DefaultMaxModelBindingRecursionDepth;
            value.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            value.PropertyNameCaseInsensitive = true;
            value.PropertyNamingPolicy = null; // leave property names unchanged (PascalCase for c#)
            value.ReadCommentHandling = JsonCommentHandling.Skip;
            value.WriteIndented = false;
        }
    }
}
