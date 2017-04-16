using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Interfaces
{
    public interface IRepositoryLoader
    {
        /// <summary>
        /// Get all ContentTypes for specified AppId. If called multiple times it loads from a private field.
        /// </summary>
        IDictionary<int, IContentType> ContentTypes(int appId);

        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        IEntity Entity(int appId, int entityId);

        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="source">DataSource to get child entities</param>
        /// <param name="entitiesOnly">If only the CachItem.Entities is needed, this can be set to true to imporove performance</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppDataPackage CompleteApp(int appId, int[] entityIds = null, IDeferredEntitiesList source = null, bool entitiesOnly = false);

        Dictionary<int, string> MetadataTargetTypes();


        Dictionary<int, Zone> Zones();
    }
}