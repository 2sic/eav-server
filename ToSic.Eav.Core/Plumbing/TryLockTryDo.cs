using System;

namespace ToSic.Eav.Plumbing;

/// <summary>
/// Execute something if a condition is met, but do it within a lock to avoid duplicate runs
/// </summary>
public class TryLockTryDo
{
    public TryLockTryDo(object lockObject = null) => _loadLock = lockObject ?? new object();

    // TODO: @STV SHOULD be applied to more places that use a lock, as I assume it's robust
    public void Do(Func<bool> condition, Action action)
    {
        if (!condition()) return;
        PreLockCount++;
        lock (_loadLock)
        {
            LockCount++;
            // Re-check, in case after the lock opened, the condition was moot
            if (condition())
                action();
        }
    }

    /// <summary>
    /// Get / Generate a value inside a lock with double-check.
    /// </summary>
    /// <param name="condition">Function to call checking if we need to generate the result</param>
    /// <param name="generator">the generator</param>
    /// <param name="cacheOrDefault">fallback to provide if no loading should happen - typically a previously cached result or default data</param>
    /// <returns></returns>
    public TResult Call<TResult>(Func<bool> condition, Func<TResult> generator, TResult cacheOrDefault)
    {
        if (!condition()) return cacheOrDefault;
        PreLockCount++;
        lock (_loadLock)
        {
            LockCount++;
            // Re-check, in case after the lock opened, the condition was moot
            if (condition())
                return generator();
        }
        return cacheOrDefault;
    }

    public int PreLockCount;
    public int LockCount;
    private readonly object _loadLock;
}