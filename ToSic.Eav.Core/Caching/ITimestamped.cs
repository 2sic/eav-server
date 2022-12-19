using ToSic.Lib.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// Provides a time stamp when something was created / updated for caching. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface ITimestamped
    {
        /// <summary>
        /// System time-stamp of when the data in this cached thing was initialized or updated.
        /// Depending on the implementation, this may go up-stream and return an up-stream value. 
        /// </summary>
        /// <returns>A timestamp as a long number</returns>
        long CacheTimestamp { get; }

    }
}
