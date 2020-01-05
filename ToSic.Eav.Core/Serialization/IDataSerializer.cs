using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Serialization
{
    /// <summary>
    /// Marks objects that can serialize EAV data like Entities or Content-Types. <br/>
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IDataSerializer
    {
        /// <summary>
        /// Initializer - necessary for most serializations
        /// </summary>
        /// <param name="appState">the app which contains the data to be serialized</param>
        /// <param name="parentLog">logger</param>
        void Initialize(AppState appState, ILog parentLog);

        /// <summary>
        /// Serialize an entity to a string
        /// </summary>
        /// <param name="entityId">ID - the serializer will look for this in the app</param>
        /// <returns>a serialized entity as string</returns>
        string Serialize(int entityId);

        /// <summary>
        /// Batch-serialize a bunch of entities
        /// </summary>
        /// <param name="entities">list of the entities</param>
        /// <returns>A string-dictionary containing all the serialized entities</returns>
        Dictionary<int, string> Serialize(List<int> entities);

        /// <summary>
        /// Serialize an entity to a string
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string Serialize(IEntity entity);

        /// <summary>
        /// Batch-serialize a bunch of entities
        /// </summary>
        /// <param name="entities">List of entities to serialize</param>
        /// <returns>A string-dictionary containing all the serialized entities</returns>
        Dictionary<int, string> Serialize(List<IEntity> entities);


    }
}
