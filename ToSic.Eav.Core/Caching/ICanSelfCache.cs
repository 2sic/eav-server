namespace ToSic.Eav.Caching
{
    public interface ICanSelfCache
    {
        // 2020-04-27.01 2dm - disabled this - as of now, it's always true, so we'll probably remove it soon
        ///// <summary>
        ///// This one will return the original result if queried again - as long as this object exists
        ///// </summary>
        //bool ReuseInitialResults { get; set; }


        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        bool AutoCaching { get; set; }

        /// <summary>
        /// Default cache duration is 3600 * 24 (1 day)
        /// </summary>
        int CacheDurationInSeconds { get; set; }

        /// <summary>
        /// Kill the cache if the source data is newer than the cache-stamped data
        /// </summary>
        bool CacheRefreshOnSourceRefresh { get; set; }

    }
}
