using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Lib.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Serialization
{
    /// <summary>
    /// Marks objects that can de-serialize EAV data like entities.
    /// </summary>
    [PrivateApi("not ready for publishing")]
    public interface IDataDeserializer: IHasLog
    {
        /// <summary>
        /// Initialize with the app. One of two possible initializers.
        /// </summary>
        /// <param name="appState">the app which contains the data to be serialized</param>
        /// <param name="parentLog">logger</param>
        void Initialize(AppState appState, ILog parentLog);

        /// <summary>
        /// Initialize with the app. One of two possible initializers. <br/>
        /// This special initialization is important for deserialization scenarios where the full app-state doesn't exist.
        /// </summary>
        /// <param name="appId">the app-ID which contains the data to be serialized</param>
        /// <param name="types">list of all content-types which the deserializer knows</param>
        /// <param name="allEntities">list of all entities to use in deserialization</param>
        /// <param name="parentLog">logger</param>
        void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource allEntities, ILog parentLog);

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
    }
}
