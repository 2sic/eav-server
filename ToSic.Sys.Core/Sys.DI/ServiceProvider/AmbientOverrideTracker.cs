namespace ToSic.Sys.DI;

public static class AmbientOverrideTracker
{
    internal sealed record Entry(
        object Key,
        object Factory,
        Entry? Parent,
        int Depth,
        string? CallerName,
        string? Hint);

    private static readonly AsyncLocal<Entry?> CurrentEntry = new();

    internal static Entry? Current
    {
        get => CurrentEntry.Value;
        set => CurrentEntry.Value = value;
    }

    public static bool HasActiveOverrides => CurrentEntry.Value is not null;

    /// <summary>
    /// Gets the total number of nested overrides currently active (0 if none).
    /// </summary>
    public static int OverrideCount => CurrentEntry.Value?.Depth ?? 0;

    public static string OverridePath => GetPath();


    internal static int Count(object key)
    {
        var count = 0;
        for (var node = CurrentEntry.Value; node is not null; node = node.Parent)
        {
            if (ReferenceEquals(node.Key, key))
                count++;
        }
        return count;
    }

    internal static string GetPath(object? key = null)
    {
        var parts = new List<string>();
        for (var node = CurrentEntry.Value; node is not null; node = node.Parent)
        {
            if (key is not null && !ReferenceEquals(node.Key, key))
                continue;
            var label = node.CallerName ?? "Unknown";
            if (node.Hint is not null)
                label += $" ({node.Hint})";
            parts.Add(label);
        }

        parts.Reverse();
        return string.Join(" > ", parts);
    }

    internal static IDisposable Push(
        object key,
        object factory,
        string? callerName = null,
        string? hint = null)
    {
        var previous = CurrentEntry.Value;
        var nextDepth = (previous?.Depth ?? 0) + 1;
        CurrentEntry.Value = new(key, factory, previous, nextDepth, callerName, hint);
        return new Scope(previous);
    }

    internal static TFactory? Find<TFactory>(object key) where TFactory : class
    {
        for (var node = CurrentEntry.Value; node is not null; node = node.Parent)
        {
            if (ReferenceEquals(node.Key, key))
                return (TFactory)node.Factory;
        }
        return null;
    }

    internal static Entry? Merge(Entry? baseChain, Entry? overlay)
    {
        if (overlay is null) return baseChain;
        if (baseChain is null || ReferenceEquals(overlay, baseChain)) return overlay;
        return PrependUnique(baseChain, overlay);
    }

    private static Entry PrependUnique(Entry baseChain, Entry current)
    {
        var baseDepth = baseChain.Depth;
        var parent = current.Parent is null || ReferenceEquals(current.Parent, baseChain)
            ? baseChain
            : PrependUnique(baseChain, current.Parent);

        var depth = parent is null ? 1 : parent.Depth + 1;
        return current with { Parent = parent, Depth = depth };
    }


    private sealed class Scope(Entry? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                CurrentEntry.Value = previous;
                _disposed = true;
            }
        }
    }
}