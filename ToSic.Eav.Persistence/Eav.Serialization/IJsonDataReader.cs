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
public partial interface IJsonDataReader: IHasLog, IServiceWithSetup<IJsonDataReader.Options>
{
    //void Initialize(Options options);

    public void ConfigureLogging(LogSettings settings);


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


    JsonEntity UnpackEntityAndTestGenericJsonV1(string? serialized);
    JsonFormat UnpackAndTestGenericJsonV1(string? serialized);
    IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType, IEntitiesSource? dynRelationshipsSource = default);
    IList<ContentTypeWithEntities> GetContentTypesFromBundles(JsonFormat package);
    IList<IEntity> GetEntitiesFromBundles(JsonFormat package, IEntitiesSource? relationshipSource = null);
    IContentType DeserializeContentType(string serialized);
    ContentTypeWithEntities ConvertContentType(JsonContentTypeSet json);
    //public IJsonDataReader Setup(Options options);
    BundleEntityWithAssets DeserializeEntityWithAssets(string serialized, bool allowDynamic = false, bool skipUnknownType = false);
    IEntity DeserializeWithRelsWip(string serialized, int id, bool allowDynamic = false, bool skipUnknownType = false, IEntitiesSource? dynRelationshipsSource = null);
    Options MyOptions { get; set; }
}