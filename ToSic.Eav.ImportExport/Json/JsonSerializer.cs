using ToSic.Eav.Serialization;
using ToSic.Eav.Metadata;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: JsonSerializerBase<JsonSerializer>, IDataDeserializer
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        public JsonSerializer(ITargetTypes metadataTargets, GlobalTypes globalTypes) : this(metadataTargets, globalTypes, "Jsn.Serlzr") {}

        protected JsonSerializer(ITargetTypes metadataTargets, GlobalTypes globalTypes, string logName): base(metadataTargets, globalTypes, logName) { }
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
