﻿using ToSic.Sys.Caching;
using ToSic.Sys.Caching.Keys;

namespace ToSic.Eav.DataSource.Sys.Caching;

/// <summary>
/// This marks an object that can provide everything necessary to
/// provide information about caching
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheInfo: ICacheKey, ICacheExpiring;