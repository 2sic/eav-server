using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Interfaces
{
    public interface IRepositoryLoader
    {
        /// <summary>
        /// Get all ContentTypes for specified AppId. If called multiple times it loads from a private field.
        /// </summary>
        IDictionary<int, IContentType> ContentTypes(int appId, IDeferredEntitiesList source);

        // 2017-06-08 disabled for now, as not in use...
        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="entitiesOnly">If only the CachItem.Entities is needed, this can be set to true to imporove performance</param>
        /// <param name="parentLog">parent logger</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppDataPackage AppPackage(int appId, int[] entityIds = null, bool entitiesOnly = false, Log parentLog = null);

        Dictionary<int, string> MetadataTargetTypes();


        Dictionary<int, Zone> Zones();
    }
}