using ToSic.Eav.Serialization;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: JsonSerializerBase<JsonSerializer>, IDataDeserializer
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        public JsonSerializer(): base("Jsn.Serlzr") {}

        protected JsonSerializer(string logName): base(logName) { }

        // todo: replace with call to Init(...)
        public JsonSerializer(AppState package, ILog parentLog): this()
        {
            Initialize(package, parentLog);
        }
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
