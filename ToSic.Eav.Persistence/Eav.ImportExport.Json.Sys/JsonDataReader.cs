using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{
    public record DeserializerOptions
    {
        public DeserializerOptions(IAppReader appReader)
        {
            AppId = appReader.AppId;
            AppReaderOrNull = appReader;
        }

        public DeserializerOptions(int appId, IEnumerable<IContentType> types)
        {
            AppId = appId;
            Types = types;
        }

        /// <summary>
        /// Initialize with the app. One of two possible initializers. <br/>
        /// This special initialization is important for deserialization scenarios where the full app-state doesn't exist.
        /// </summary>
        /// <param name="appId">the app-ID which contains the data to be serialized</param>
        /// <param name="types">list of all content-types which the deserializer knows</param>
        /// <param name="lazyRelationshipLookupList">list of all entities to use in deserialization</param>
        public DeserializerOptions(int appId, IEnumerable<IContentType> types, IEntitiesSource? lazyRelationshipLookupList)
        {
            AppId = appId;
            Types = types;
            LazyRelationshipLookupList = lazyRelationshipLookupList;
        }

        public int AppId { get; }
        public IAppReader? AppReaderOrNull { get; }

        public IEnumerable<IContentType>? Types { get; private init; }

        public IEntitiesSource? LazyRelationshipLookupList
        {
            get => field ??= (AppReaderOrNull ?? throw new("cannot use app in serializer without initializing it first, make sure you call Initialize(...)")).AppState;
            private init;
        }

        /// <summary>
        /// Ability to inject a different TransientContentTypeGenerator and other parameters
        /// just for deserialization
        /// </summary>
        public JsonDeSerializationSettings? DeserializationSettings { get; init; } = null;

        public bool PreferLocalAppTypes { get; init; }

        public bool AssumeUnknownTypesAreDynamic { get; init; } = false;

    }

    public DeserializerOptions DesOptions
    {
        get => field ?? throw new("Can't use DeserializationOptions - you forgot to call Init");
        set;
    } = null!;


    //public void Initialize(IAppReader appReader)
    //{
    //    AppReaderOrNull = appReader;
    //    AppId = appReader.AppId;
    //}
    public void Initialize(DeserializerOptions options)
    {
        DesOptions = options;
    }
    public IJsonDataReader Setup(DeserializerOptions options)
    {
        DesOptions = options;
        return this;
    }

    //protected int AppId;

    //public void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource? allEntities)
    //{
    //    AppId = appId;
    //    _types = types;
    //    _relList = allEntities;
    //}

    //protected IEntitiesSource LazyRelationshipLookupList => _relList ??= (AppReaderOrNull ?? throw new("cannot use app in serializer without initializing it first, make sure you call Initialize(...)")).AppState;
    //private IEntitiesSource? _relList;

    //protected IAppReader? AppReaderOrNull { get; private set; }

    ///// <summary>
    ///// Ability to inject a different TransientContentTypeGenerator and other parameters
    ///// just for deserialization
    ///// </summary>
    //internal JsonDeSerializationSettings? DeserializationSettings { get; set; } = null;

    private JsonDeserializeEntityHelper JsonDeserializeEntityHelper => field ??= new(services.GlobalData, services.ContentTypeAssemblyKit, DesOptions, LogDsDetails);
}
