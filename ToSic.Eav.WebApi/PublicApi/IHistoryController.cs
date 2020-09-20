using System.Collections.Generic;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IHistoryController
    {
        /// <summary>
        /// Get the history of an item
        /// </summary>
        List<ItemHistory> Get(int appId, ItemIdentifier item);

        /// <summary>
        /// Restore an item to a previous version in the history
        /// </summary>
        bool Restore(int appId, int changeId, ItemIdentifier item);
    }
}