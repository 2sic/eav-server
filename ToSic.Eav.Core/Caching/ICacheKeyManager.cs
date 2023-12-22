using ToSic.Lib.Documentation;

namespace ToSic.Eav.Caching;

/// <summary>
/// Generates cache-keys for certain objects and combines them with CacheKeys of parent-objects which this object relies on.  
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICacheKeyManager: ICacheKey
{
    string[] SubKeys { get; }
}