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
    private readonly object _selectionSync = new();
    private bool _legacyCreatedBeforeSelection;

    // Boot logging can start before host DI is ready, so Legacy must remain the safe default.
    internal ILogFactory Current => Volatile.Read(ref _selected) ?? legacyFactory;

    internal void Select(ILogFactory factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));
        lock (_selectionSync)
        {
            if (_legacyCreatedBeforeSelection && !ReferenceEquals(factory, legacyFactory))
                throw new InvalidOperationException("A Legacy log was created before MEL was selected.");
            if (_selected != null && !ReferenceEquals(_selected, factory))
                throw new InvalidOperationException("A log factory was already selected for this application.");
            Volatile.Write(ref _selected, factory);
        }
    }

    internal ILogFactory For(ILog? parent)
    {
        var realParent = parent.GetRealLog();
        if (realParent == null && Volatile.Read(ref _selected) == null)
        {
            // A root created before host selection commits this process to Legacy.
            lock (_selectionSync)
            {
                if (_selected == null)
                {
                    _legacyCreatedBeforeSelection = true;
                    return legacyFactory;
                }
            }
        }
        var factory = realParent switch
        {
            Log => legacyFactory,
            ILogFactoryOwner owner => owner.Factory,
            _ => Current
        };
        if (realParent != null && Volatile.Read(ref _selected) != null
            && !ReferenceEquals(factory, Current))
            throw new InvalidOperationException("A log parent belongs to a different logging stack.");
        return factory;
    }
}
