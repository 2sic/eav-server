namespace ToSic.Lib.Helpers;

/// <summary>
/// Execute something if a condition is met, but do it within a lock to avoid duplicate runs
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class TryLockTryDo(object? lockObject = null)
{
    /// <summary>
    /// The real lock - either use the provided one or create a new one,
    /// in which case sharing the locks is not expected.
    /// </summary>
    private readonly object _loadLock = lockObject ?? new object();

    public int PreLockCount;
    public int LockCount;

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
    /// <param name="conditionToGenerate">Function to call checking if we need to generate the result</param>
    /// <param name="generator">the generator</param>
    /// <param name="cacheOrFallback">fallback to provide if no loading should happen - typically a previously cached result or default data</param>
    /// <returns></returns>
    public (TResult Result, bool Generated, string Message) Call<TResult>(Func<bool> conditionToGenerate, Func<TResult> generator, Func<TResult> cacheOrFallback)
    {
        if (!conditionToGenerate())
            return (cacheOrFallback(), false, "fallback; after 1st condition check");

        PreLockCount++;
        lock (_loadLock)
        {
            LockCount++;
            // Re-check, in case after the lock opened, the condition was moot
            if (conditionToGenerate())
                return (generator(), true, "generated");
        }
        return (cacheOrFallback(), false, "fallback; after 2nd condition checks");
    }

}