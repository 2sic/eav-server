using System;

namespace ToSic.Eav.Plumbing
{
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

        [Obsolete("warning - not really obsolete, but never tested yet, so if you use this, do verify it works")]
        public TResult Call<TResult>(Func<bool> condition, Func<TResult> func, TResult fallback)
        {
            if (!condition()) return fallback;
            PreLockCount++;
            lock (_loadLock)
            {
                LockCount++;
                // Re-check, in case after the lock opened, the condition was moot
                if (condition())
                    return func();
            }
            return fallback;
        }

        public int PreLockCount;
        public int LockCount;
        private readonly object _loadLock;
    }
}
