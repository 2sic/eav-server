﻿using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;

namespace ToSic.Eav.Serialization;

/// <summary>
/// Marks objects that can de-serialize EAV data like entities.
/// </summary>
[PrivateApi("not ready for publishing")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDataDeserializer: IHasLog
{
    ///// <summary>
    ///// Initialize with the app. One of two possible initializers.
    ///// </summary>
    ///// <param name="appState">the app which contains the data to be serialized</param>
    //void Initialize(AppState appState);

    void Initialize(IAppReader appState);

    /// <summary>
    /// Initialize with the app. One of two possible initializers. <br/>
    /// This special initialization is important for deserialization scenarios where the full app-state doesn't exist.
    /// </summary>
    /// <param name="appId">the app-ID which contains the data to be serialized</param>
    /// <param name="types">list of all content-types which the deserializer knows</param>
    /// <param name="allEntities">list of all entities to use in deserialization</param>
    void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource allEntities);

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


    /// <summary>
    /// De-serialize many entities.
    /// </summary>
    /// <param name="serialized">The original serialized entity</param>
    /// <param name="allowDynamic">
    /// Determines if de-serialization may also work on entities without a known type in the app-state. <br/>
    /// This is important for de-serializing objects without a pre-defined type. <br/>
    /// This is used for the dynamic entities and global entities which are loaded before most types have been initialized. 
    /// </param>
    /// <returns>A list of entity objects</returns>
    List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false);


    /// <summary>
    /// De-serialize ContentTypeAttributeSysSettings from SysSettings string field in ToSicEavAttributes and ToSicEavAttributeSets (EF/DB)
    /// </summary>
    /// <param name="serialized"></param>
    /// <returns>ContentTypeAttributeSysSettings or null</returns>
    ContentTypeAttributeSysSettings DeserializeAttributeSysSettings(string serialized);


    /// <summary>
    /// Serialize ContentTypeAttributeSysSettings
    /// </summary>
    /// <param name="sysSettings"></param>
    /// <returns>string or null</returns>
    string Serialize(ContentTypeAttributeSysSettings sysSettings);
}