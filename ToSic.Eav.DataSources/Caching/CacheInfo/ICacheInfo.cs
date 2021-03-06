﻿using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSources.Caching.CacheInfo
{
    /// <summary>
    /// This marks an object that can provide everything necessary to
    /// provide information about caching
    /// </summary>
    public interface ICacheInfo: ICacheKey, ICacheExpiring
    {
    }
}
