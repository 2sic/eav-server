using Newtonsoft.Json;
using ToSic.Eav.Serialization;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: SerializerBase, IDataDeserializer
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        public JsonSerializer() : base("Jsn.Serlzr") {}

        public JsonSerializer(AppState package, ILog parentLog): this()
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
