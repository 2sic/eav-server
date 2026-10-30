using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.Serialization;

/// <summary>
/// Marks objects that can serialize EAV data like Entities or Content-Types. <br/>
/// </summary>
[PrivateApi("not ready for publishing")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IJsonDataWriter // formerly IDataSerializer
{
    void Initialize(JsonSerializer.DeserializerOptions options);

    //void Initialize(IAppReader appReader);

    ///// <summary>
    ///// Serialize an entity to a string - NOTE: ONLY SEEMS TO BE USED IN TESTS
    ///// </summary>
    ///// <param name="entityId">ID - the serializer will look for this in the app</param>
    ///// <returns>a serialized entity as string</returns>
    //string Serialize(int entityId);

    /// <summary>
    /// Serialize an entity to a string
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    string Serialize(IEntity entity);

    ///// <summary>
    ///// Batch-serialize a bunch of entities
    ///// </summary>
    ///// <param name="entities">List of entities to serialize</param>
    ///// <returns>A string-dictionary containing all the serialized entities</returns>
    //Dictionary<int, string> Serialize(List<IEntity> entities);


    JsonFormat ToPackage(IContentType contentType, JsonSerializationSettings settings);
    JsonContentType ToJson(IContentType contentType);
    JsonEntity? ToJson(IEntity? entity, int metadataDepth = 0);

    /// <summary>
    /// WIP test API to ensure content-types serialized for UI resolve any hyperlinks.
    /// This is ATM only relevant to ensure that file-references in the WYSIWYG CSS work
    /// See https://github.com/2sic/2sxc/issues/2930
    /// Otherwise it's not used, so don't publish this API anywhere
    /// </summary>
    bool ValueConvertHyperlinks { get; set; }

    string Serialize(BundleEntityWithAssets bundle, int metadataDepth);
    JsonSerializer Setup(JsonSerializer.DeserializerOptions options);
}