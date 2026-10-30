using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.ImportExport.Json.Sys;

public record JsonDataReadOptions
{
    public JsonDataReadOptions(IAppReader appReader)
    {
        AppId = appReader.AppId;
        AppReaderOrNull = appReader;
    }

    public JsonDataReadOptions(int appId, IEnumerable<IContentType> types)
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
    public JsonDataReadOptions(int appId, IEnumerable<IContentType> types, IEntitiesSource? lazyRelationshipLookupList)
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
