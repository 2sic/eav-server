using System;
using System.Collections.Generic;
using System.Web.Http;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntitiesController
    {
        /// <summary>
        /// Get a single item for the UI
        /// </summary>
        Dictionary<string, object> GetOne(string contentType, int id, int appId, string cultureCode = null);

        /// <summary>
        /// Get a lot of entities / items to edit in the UI
        /// Used for the old AngularJS UI, will be deprecated once the new system is live
        /// </summary>
        dynamic GetManyForEditing([FromBody] List<ItemIdentifier> items, int appId);

        /// <summary>
        /// Save many items
        /// Used in the old AngularJS UI, will be depricated once the new system is live
        /// </summary>
        Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<BundleWithHeader<EntityWithLanguages>> items, [FromUri] bool partOfPage = false);

        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
        IEnumerable<Dictionary<string, object>> GetEntities(string contentType, int appId, string cultureCode = null);

        IEnumerable<Dictionary<string, object>> GetAllOfTypeForAdmin(int appId, string contentType);

        void Delete(string contentType, int id, int appId, bool force = false);

        void Delete(string contentType, Guid guid, int appId, bool force = false);

        /// <summary>
        /// Get a ContentType by Name
        /// </summary>
        ContentTypeInfo GetContentType(string contentType, int appId);

        /// <summary>
        /// Get the history of an item
        /// </summary>
        List<ItemHistory> History(int appId, [FromBody]ItemIdentifier item);

        /// <summary>
        /// Restore an item to a previous version in the history
        /// </summary>
        bool Restore(int appId, int changeId, [FromBody]ItemIdentifier item);
    }
}