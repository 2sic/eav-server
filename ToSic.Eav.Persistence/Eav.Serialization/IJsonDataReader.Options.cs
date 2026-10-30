using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.Sys;

namespace ToSic.Eav.Serialization;

partial interface IJsonDataReader
{
    public record Options
    {
        public Options(IAppReader appReader)
        {
            AppId = appReader.AppId;
            AppReaderOrNull = appReader;
        }

        /// <summary>
        /// Initialize with the app. One of two possible initializers. <br/>
        /// This special initialization is important for deserialization scenarios where the full app-state doesn't exist.
        /// </summary>
        /// <param name="appId">the app-ID which contains the data to be serialized</param>
        /// <param name="types">list of all content-types which the deserializer knows</param>
        public Options(int appId, IEnumerable<IContentType> types)
        {
            AppId = appId;
            Types = types;
        }

        public int AppId { get; }
        public IAppReader? AppReaderOrNull { get; }

        public IEnumerable<IContentType>? Types { get; private init; }

        /// <summary>
        /// list of all entities to use in deserialization.
        /// Will take preference; if not provided, the AppReader will be used, in which case it's then required.
        /// </summary>
        public IEntitiesSource? LazyRelationshipLookupList
        {
            get => field
                ??= (AppReaderOrNull
                     ?? throw new("cannot use app in serializer without initializing it first, make sure you call Initialize(...)")
                ).AppState;
            init;
        }

        /// <summary>
        /// Ability to inject a different TransientContentTypeGenerator and other parameters
        /// just for deserialization
        /// </summary>
        public JsonDeSerializationSettings? DeserializationSettings { get; init; } = null;

        public bool PreferLocalAppTypes { get; init; }

        public bool AssumeUnknownTypesAreDynamic { get; init; } = false;

    }
}
