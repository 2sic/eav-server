namespace ToSic.Eav.Interfaces
{
    public interface ICanSelfCache
    {

        /// <summary>
        /// This one will return the original result if queried again - as long as this object exists
        /// </summary>
        bool ReuseInitialResults { get; set; }


        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        bool AutoCaching { get; set; }

        /// <summary>
        /// Default cache duration is 3600
        /// </summary>
        int CacheDurationInSeconds { get; set; }

        /// <summary>
        /// Kill the cache if the source data is newer than the cache-stamped data
        /// </summary>
        bool CacheRefreshOnSourceRefresh { get; set; }

    }
}
