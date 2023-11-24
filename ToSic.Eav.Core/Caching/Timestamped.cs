namespace ToSic.Eav.Caching;

/// <summary>
/// Very simple timestamp holder, mainly for Delegated Cache-Expiry to hold the private cache timestamp.
/// </summary>
public class Timestamped: ITimestamped
{
    public long CacheTimestamp { get; set; }
}