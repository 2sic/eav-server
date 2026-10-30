using ToSic.Eav.Data.ContentTypes.Fields.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.Serialization;

/// <summary>
/// Marks objects that can de-serialize EAV data like entities.
/// </summary>
[PrivateApi("not ready for publishing")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IJsonDataReader: IHasLog
{
    //void Initialize(IAppReader appReader);
    void Initialize(JsonSerializer.DeserializerOptions options);

    public void ConfigureLogging(LogSettings settings);


    ///// <summary>
    ///// Initialize with the app. One of two possible initializers. <br/>
    ///// This special initialization is important for deserialization scenarios where the full app-state doesn't exist.
    ///// </summary>
    ///// <param name="appId">the app-ID which contains the data to be serialized</param>
    ///// <param name="types">list of all content-types which the deserializer knows</param>
    ///// <param name="allEntities">list of all entities to use in deserialization</param>
    //void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource? allEntities);

    /// <summary>
    /// De-serialize an entity.
    /// </summary>
    /// <param name="serialized">The original serialized entity</param>
    /// <param name="allowDynamic">
    /// Determines if de-serialization may also work on entities without a known type in the app-state. <br/>
    /// This is important for de-serializing objects without a pre-defined type. <br/>
    /// This is used for the dynamic entities and global entities which are loaded before most types have been initialized. 
    /// </param>
    /// <param name="skipUnknownType">
    /// If unknown types should be skipped. This will prevent an error if a type isn't known, and allowDynamic is false.
    /// </param>
    /// <returns>An entity object</returns>
    IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false);


    ///// <summary>
    ///// De-serialize many entities.
    ///// </summary>
    ///// <param name="serialized">The original serialized entity</param>
    ///// <param name="allowDynamic">
    /////     Determines if de-serialization may also work on entities without a known type in the app-state. <br/>
    /////     This is important for de-serializing objects without a pre-defined type. <br/>
    /////     This is used for the dynamic entities and global entities which are loaded before most types have been initialized. 
    ///// </param>
    ///// <returns>A list of entity objects</returns>
    //IList<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false);

    /// <summary>
    /// Serialize ContentTypeFieldSysSettings
    /// </summary>
    /// <param name="sysSettings"></param>
    /// <returns>string or null</returns>
    //public string? Serialize(ContentTypeFieldSysSettings sysSettings);

    JsonEntity UnpackEntityAndTestGenericJsonV1(string? serialized);
    JsonFormat UnpackAndTestGenericJsonV1(string? serialized);
    IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType, IEntitiesSource? dynRelationshipsSource = default);
    IList<ContentTypeWithEntities> GetContentTypesFromBundles(JsonFormat package);
    IList<IEntity> GetEntitiesFromBundles(JsonFormat package, IEntitiesSource? relationshipSource = null);
    IContentType DeserializeContentType(string serialized);
    ContentTypeWithEntities ConvertContentType(JsonContentTypeSet json);
    public IJsonDataReader Setup(JsonSerializer.DeserializerOptions options);
    BundleEntityWithAssets DeserializeEntityWithAssets(string serialized, bool allowDynamic = false, bool skipUnknownType = false);
    IEntity DeserializeWithRelsWip(string serialized, int id, bool allowDynamic = false, bool skipUnknownType = false, IEntitiesSource? dynRelationshipsSource = null);
    JsonSerializer.DeserializerOptions DesOptions { get; set; }
}