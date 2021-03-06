﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Repositories
{
    /// <summary>
    /// Minimal state loader - can only load parts that an app can load, so content-types and entities
    /// </summary>
    public interface IAppRepositoryLoader
    {
        /// <summary>
        /// Real constructor, after DI
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="path"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        IAppRepositoryLoader Init(int appId, string path, ILog log);

        /// <summary>
        /// Get all ContentTypes for specified AppId.
        /// </summary>
        /// <param name="entitiesSource"></param>
        IList<IContentType> ContentTypes(IEntitiesSource entitiesSource);
    }
}
