using ToSic.Eav.Serialization;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: JsonSerializerBase<JsonSerializer>, IDataDeserializer
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        public JsonSerializer(ITargetTypes metadataTargets) : this(metadataTargets, "Jsn.Serlzr") {}

        protected JsonSerializer(ITargetTypes metadataTargets, string logName): base(metadataTargets, logName) { }
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
