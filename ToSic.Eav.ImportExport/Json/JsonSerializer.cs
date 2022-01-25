using ToSic.Eav.Apps;
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
        public JsonSerializer(ITargetTypes metadataTargets, IAppStates appStates) : this(metadataTargets, appStates, "Jsn.Serlzr") {}

        protected JsonSerializer(ITargetTypes metadataTargets, IAppStates appStates, string logName): base(metadataTargets, appStates, logName) { }
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
