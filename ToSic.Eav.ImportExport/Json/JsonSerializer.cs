using ToSic.Eav.Apps;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Serialization;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: JsonSerializerBase<JsonSerializer>, IDataDeserializer
    {
        private readonly EntityBuilder _entityBuilder;
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        /// <summary>
        /// Constructor for DI
        /// </summary>
        public JsonSerializer(ITargetTypes metadataTargets, IAppStates appStates, EntityBuilder entityBuilder) : this(metadataTargets, appStates, entityBuilder, "Jsn.Serlzr") {}
        

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        protected JsonSerializer(ITargetTypes metadataTargets, IAppStates appStates, EntityBuilder entityBuilder, string logName): base(metadataTargets, appStates, logName)
        {
            _entityBuilder = entityBuilder;
        }
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
