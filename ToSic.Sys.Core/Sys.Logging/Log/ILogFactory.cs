namespace ToSic.Sys.Logging;

/// <summary>
/// Creates logs while preserving their implementation across parent-child chains.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILogFactory
{
    /// <summary>
    /// Creates a log.
    /// </summary>
    ILog Create(string name, ILog? parent, CodeRef code, string? initialMessage = default);
}

internal interface ILogFactoryOwner
{
    ILogFactory Factory { get; }
}

internal sealed class LogFactorySelector(ILogFactory legacyFactory)
{
    private ILogFactory? _selected;

    // Boot logging can start before host DI is ready, so Legacy must remain the safe default.
    internal ILogFactory Current => Volatile.Read(ref _selected) ?? legacyFactory;

    internal void Select(ILogFactory factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));
        // The application must not mix implementations after normal logging has started.
        var previous = Interlocked.CompareExchange(ref _selected, factory, null);
        if (previous != null && !ReferenceEquals(previous, factory))
            throw new InvalidOperationException("A log factory was already selected for this application.");
    }

    internal ILogFactory For(ILog? parent)
    {
        // A known parent wins, so a child cannot cross from Legacy to MEL or the other way around.
        var realParent = parent.GetRealLog();
        return realParent switch
        {
            Log => legacyFactory,
            ILogFactoryOwner owner => owner.Factory,
            _ => Current
        };
    }
}
