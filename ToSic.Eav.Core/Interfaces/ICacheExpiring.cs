namespace ToSic.Eav.Interfaces
{
    public interface ICacheExpiring
    {
        /// <summary>
        /// Sistem time of last refresh
        /// </summary>
        long CacheTimestamp { get; }

        /// <summary>
        /// Find out if it changed
        /// </summary>
        /// <param name="prevCacheTimestamp"></param>
        /// <returns></returns>
        bool CacheChanged(long prevCacheTimestamp);

    }
}
