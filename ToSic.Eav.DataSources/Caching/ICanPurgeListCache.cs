using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Marks objects which can purge it's own cache, and also force upstream caches to be purged. <br/>
    /// This helps in scenarios where the code knows that the cache should be cleaned, but needs to rely on the whole tree to be cleaned.
    /// Without this, a cache would be cleared but the next-upstream would still be cached, so the next access would still return the same results. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface ICanPurgeListCache
    {
        /// <summary>
        /// Remove the current data from the cache, optionally also purge everything upstream
        /// </summary>
        /// <param name="cascade">true to purge all sources as well, default is false</param>
        void PurgeList(bool cascade = false);

    }
}
