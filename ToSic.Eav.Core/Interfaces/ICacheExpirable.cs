namespace ToSic.Eav.Interfaces
{
    public interface ICacheExpirable
    {
        ///// <summary>
        ///// Sistem time of last refresh
        ///// </summary>
        //long CacheTimestamp { get; }

        /// <summary>
        /// Reset the time
        /// </summary>
        void CacheResetTimestamp();

        ///// <summary>
        ///// Find out if it changed
        ///// </summary>
        ///// <param name="prevCacheTimestamp"></param>
        ///// <returns></returns>
        //bool CacheChanged(long prevCacheTimestamp);

    }
}
