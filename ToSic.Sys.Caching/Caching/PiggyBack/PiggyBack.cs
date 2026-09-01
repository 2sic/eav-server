using System.Collections.Concurrent;

namespace ToSic.Sys.Caching.PiggyBack;

/// <summary>
/// Object to provide a simple value cache to certain objects.
/// For things which shouldn't be constantly generated / looked up.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PiggyBack() : ConcurrentDictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
