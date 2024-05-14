using System.Collections.Concurrent;

namespace ToSic.Eav.Caching;

/// <summary>
/// Very simple helper to manage locks - typically for caching or resource intensive generation operations.
/// </summary>
/// <param name="ignoreCase"></param>
[PrivateApi]
public class NamedLocks(bool ignoreCase = true)
{
    public readonly ConcurrentDictionary<string, object> Locks = new(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

    public object Get(string key) => Locks.GetOrAdd(key, _ => new());

}