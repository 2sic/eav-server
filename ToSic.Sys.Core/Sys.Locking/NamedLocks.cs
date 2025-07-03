using System.Collections.Concurrent;

namespace ToSic.Sys.Locking;

/// <summary>
/// Very simple helper to manage locks - typically for caching or resource intensive generation operations.
/// </summary>
/// <param name="ignoreCase"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class NamedLocks(bool ignoreCase = true)
{
    public readonly ConcurrentDictionary<string, object> Locks = new(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

    public object Get(string key) => Locks.GetOrAdd(key, _ => new());

}