namespace ToSic.Eav.Interfaces.Caches
{
    public interface ICacheKeyProvider
    {
        /// <summary>
        /// Unique key-id for this specific part
        /// </summary>
        string CachePartialKey { get; }

        /// <summary>
        /// Merged key containing partial key and upstream keys
        /// </summary>
        string CacheFullKey { get; }

    }
}
