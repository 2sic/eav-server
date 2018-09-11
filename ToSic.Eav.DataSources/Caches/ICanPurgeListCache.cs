namespace ToSic.Eav.DataSources.Caches
{
    public interface ICanPurgeListCache
    {
        /// <summary>
        /// Remove this from the cache, optionally also purge everything upstream
        /// </summary>
        /// <param name="cascade">true to purge all sources as well, default is false</param>
        void PurgeList(bool cascade = false);

    }
}
