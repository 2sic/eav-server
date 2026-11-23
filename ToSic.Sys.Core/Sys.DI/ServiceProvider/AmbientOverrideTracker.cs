namespace ToSic.Sys.DI;

public static class AmbientOverrideTracker
{
    private static readonly AsyncLocal<int> ActiveOverrideCount = new();

    public static bool HasActiveOverrides => ActiveOverrideCount.Value > 0;

    internal static IDisposable Increment()
    {
        var previous = ActiveOverrideCount.Value;
        ActiveOverrideCount.Value = previous + 1;
        return new Scope(previous);
    }

    private sealed class Scope(int previousValue) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            ActiveOverrideCount.Value = previousValue;
            _disposed = true;
        }
    }
}
