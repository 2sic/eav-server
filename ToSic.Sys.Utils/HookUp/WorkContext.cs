namespace ToSic.Sys.HookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record WorkContext
{
    /// <summary>
    /// Just an empty constructor to see where it's used.
    /// </summary>
    public WorkContext() { }
    
    internal Dictionary<string, object?> Items { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public virtual TContext Get<TContext>(string name)
    {
        if (Items.TryGetValue(name, out var context))
            return (TContext)context!;

        throw new KeyNotFoundException($"Context '{name}' of type {typeof(TContext)} not found.");
    }
}