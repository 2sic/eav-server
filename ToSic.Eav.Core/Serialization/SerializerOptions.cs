using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ToSic.Eav.Serialization
{
    public static class SerializerOptions
    {
        /// <summary>
        /// Compared to the default encoder, the UnsafeRelaxedJsonEscaping encoder is more permissive about allowing characters to pass through unescaped:
        /// - It doesn't escape HTML-sensitive characters such as <, >, &, and '.
        /// - It doesn't offer any additional defense-in-depth protections against XSS or information disclosure attacks, such as those which might result from the client and server disagreeing on the charset.
        /// Use the unsafe encoder only when it's known that the client will be interpreting the resulting payload as UTF-8 encoded JSON.
        /// For example, you can use it if the server is sending the response header Content-Type: application/json; charset=utf-8.
        /// Never allow the raw UnsafeRelaxedJsonEscaping output to be emitted into an HTML page or a <script> element.
        /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-encoding#serialize-all-characters
        /// </summary>
        public static JsonSerializerOptions SxcUnsafeJsonSerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            Converters = { new Plumbing.DateTimeConverter(), new JsonStringEnumConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IncludeFields = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null, // leave property names unchanged (PascalCase for c#)
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
        };

        // this one shoudl be safe for injecting in html
        public static JsonSerializerOptions SxcAttributeJsonSerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            Converters = { new Plumbing.DateTimeConverter(), new JsonStringEnumConverter() },
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CurrencySymbols/*, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB, UnicodeRanges.LatinExtendedC, UnicodeRanges.LatinExtendedD, UnicodeRanges.LatinExtendedE, UnicodeRanges.LatinExtendedAdditional*/),
            IncludeFields = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null, // leave property names unchanged (PascalCase for c#)
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
        };

        public static JsonSerializerOptions FeaturesJsonSerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            Converters = { new Plumbing.ShortDateTimeConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null, // leave property names unchanged (PascalCase for c#)
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
        };
    }
}
