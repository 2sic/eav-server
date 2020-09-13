using System;
using System.Collections.Generic;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntitiesController
    {
        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
        IEnumerable<Dictionary<string, object>> GetEntities(string contentType, int appId/*, string cultureCode = null*/);

        IEnumerable<Dictionary<string, object>> GetAllOfTypeForAdmin(int appId, string contentType);

        void Delete(string contentType, int id, int appId, bool force = false);

        void Delete(string contentType, Guid guid, int appId, bool force = false);

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