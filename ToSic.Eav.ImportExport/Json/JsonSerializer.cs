using Newtonsoft.Json;
using ToSic.Eav.App;
using ToSic.Eav.ImportExport.Serializers;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: SerializerBase
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        public JsonSerializer() : base("Jsn.Serlzr") {}

        public JsonSerializer(AppDataPackage package, Log parentLog): this()
        {
            Initialize(package, parentLog);
        }

        private static JsonSerializerSettings JsonSerializerSettings()
            => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
