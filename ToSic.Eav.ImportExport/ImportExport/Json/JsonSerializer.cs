using ToSic.Eav.Apps;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: JsonSerializerBase<JsonSerializer>, IDataDeserializer
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Constructor for DI
        /// </summary>
        public JsonSerializer(ITargetTypes metadataTargets, IAppStates appStates, MultiBuilder multiBuilder) : this(metadataTargets, appStates, multiBuilder, "Jsn.Serlzr") {}
        

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        protected JsonSerializer(ITargetTypes metadataTargets, IAppStates appStates, MultiBuilder multiBuilder, string logName): base(metadataTargets, appStates, logName)
        {
            MultiBuilder = multiBuilder;
        }
        protected MultiBuilder MultiBuilder { get; }
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
