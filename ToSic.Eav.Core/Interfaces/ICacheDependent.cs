namespace ToSic.Eav.Interfaces
{
    public interface ICacheDependent
    {
        /// <summary>
        /// Sistem time of last refresh
        /// </summary>
        long CacheTimestamp { get; }

        /// <summary>
        /// Find out if it changed
        /// </summary>
        /// <returns></returns>
        bool CacheChanged();
    }
}
