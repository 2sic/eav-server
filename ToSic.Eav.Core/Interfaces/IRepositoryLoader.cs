using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Interfaces
{
    public interface IRepositoryLoader
    {
        /// <summary>
        /// Get all ContentTypes for specified AppId. If called multiple times it loads from a private cache.
        /// </summary>
        IList<IContentType> ContentTypes(int appId, IDeferredEntitiesList source);

        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="entityIds">null or a List of EntitiIds</param>
        /// <param name="parentLog">parent logger</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppDataPackage AppPackage(int appId, int[] entityIds = null, Log parentLog = null);

        AppDataPackage Update(AppDataPackage app, AppPackageLoadingSteps startAt, int[] entityIds = null, Log parentLog = null);


        Dictionary<int, Zone> Zones();
    }
}