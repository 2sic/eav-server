using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// Generates cache-keys for certain objects and combines them with CacheKeys of parent-objects which this object relies on.  
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface ICacheKeyManager: ICacheKey
    {

        //string[] DependentFullKeys { get; }
        string[] SubKeys { get; }

        //string FullKey { get; }
    }
}
