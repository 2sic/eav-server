using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Marks objects which can purge it's own cache, and also force upstream caches to be purged.
    /// </summary>
    [PrivateApi("probably will change namespace 'Caches' to 'Cache' or 'Caching'")]
    public interface ICanPurgeListCache
    {
        /// <summary>
        /// Remove the current data from the cache, optionally also purge everything upstream
        /// </summary>
        /// <param name="cascade">true to purge all sources as well, default is false</param>
        void PurgeList(bool cascade = false);

    }
}
