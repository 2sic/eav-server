using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Repositories
{
    /// <summary>
    /// Minimal state loader - can only load parts that an app can load, so content-types and entities
    /// </summary>
    public interface IAppRepositoryLoader
    {
        /// <summary>
        /// Get all ContentTypes for specified AppId.
        /// </summary>
        IList<IContentType> ContentTypes();
    }
}
