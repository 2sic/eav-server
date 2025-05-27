namespace ToSic.Lib.Caching.Statistics;

/// <summary>
/// WIP - trying to keep more information about cache changes
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheStatistics: ITimestamped
{
    long FirstTimestamp { get; }
        
    Stack<CacheHistory> History { get; }
        
    int ResetCount { get; }

    void Update(long newTimeStamp, int itemCount, string message);
}