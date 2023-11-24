﻿using System.Collections.Generic;

namespace ToSic.Eav.Caching;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CacheStatistics: ICacheStatistics
{
    public long CacheTimestamp { get; private set; }
        
    public long FirstTimestamp { get; private set; }
        
    public Stack<CacheHistory> History { get; } = new();

    public int ResetCount { get; private set; }

    public void Update(long newTimeStamp, int itemCount, string message)
    {
        CacheTimestamp = newTimeStamp;
        if (FirstTimestamp == 0) FirstTimestamp = newTimeStamp;
        else ResetCount++;
        History.Push(new CacheHistory { Timestamp = newTimeStamp, ResetCount = ResetCount, ItemCount = itemCount, Message = message });
    }
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public struct CacheHistory
{
    public long Timestamp;
    public int ItemCount;
    public int ResetCount;
    public string Message;
}