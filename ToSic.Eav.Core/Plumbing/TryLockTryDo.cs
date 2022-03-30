using System;

namespace ToSic.Eav.Plumbing
{
    /// <summary>
    /// Execute something if a condition is met, but do it within a lock to avoid duplicate runs
    /// </summary>
    public class TryLockTryDo
    {
        public TryLockTryDo(object lockObject = null) 
            => _loadLock = lockObject ?? new object();

        public void Go(Func<bool> condition, Action action)
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

        public int PreLockCount;
        public int LockCount;
        private readonly object _loadLock;
    }
}
