using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Interfaces
{
    public interface IRepositoryLoader
    {
        /// <summary>
        /// Get all ContentTypes for specified AppId.
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
        AppDataPackage AppPackage(int appId, int[] entityIds = null, ILog parentLog = null);

        AppDataPackage Update(AppDataPackage app, AppPackageLoadingSteps startAt, int[] entityIds = null, ILog parentLog = null);


        Dictionary<int, Zone> Zones();

        string PrimaryLanguage { get; set; }
    }
}