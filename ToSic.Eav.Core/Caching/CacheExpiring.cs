﻿namespace ToSic.Eav.Caching;

/// <summary>
/// Very simple timestamp holder, mainly for Delegated Cache-Expiry to hold the private cache timestamp.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CacheExpiring: ITimestamped, ICacheExpiring
{
    public long CacheTimestamp { get; set; }

    public bool CacheChanged(long dependentTimeStamp) => CacheTimestamp > dependentTimeStamp;
}