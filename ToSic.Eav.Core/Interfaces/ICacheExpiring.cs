using ToSic.Eav.Documentation;

namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// Marks objects which are cache-based, and which may contain obsolete cached data.
    /// </summary>
    [PublicApi]
    public interface ICacheExpiring
    {
        /// <summary>
        /// System time-stamp of when the data in this cached thing was initialized. 
        /// </summary>
        /// <returns>A timestamp as a long number</returns>
        long CacheTimestamp { get; }

        /// <summary>
        /// Detect if the cache has old data, by comparing it to a timestamp which may be newer. <br/>
        /// This is implemented in each object, because sometimes it compares its own timestamp, sometimes that of another underlying object.
        /// </summary>
        /// <param name="newCacheTimeStamp">New time stamp to compare with</param>
        /// <returns>True if the timestamps differ, false if it's the same</returns>
        bool CacheChanged(long newCacheTimeStamp);

    }
}
