namespace ToSic.Sys.HookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record WorkContext
{
    /// <summary>
    /// Just an empty constructor to see where it's used.
    /// </summary>
    public WorkContext() { }

    #region Attached HookUp Engine

    public IHookUpWork HookUp { get; init; }

    #endregion

    #region Context Values / Properties

    internal Dictionary<string, object?> Items { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public virtual TContext Get<TContext>()
        => Get<TContext>(typeof(TContext).Name);
    public virtual TContext? TryGet<TContext>()
        => TryGet<TContext>(typeof(TContext).Name);
    
    public virtual TContext Get<TContext>(string name)
    {
        if (Items.TryGetValue(name, out var context))
            return (TContext)context!;

        throw new KeyNotFoundException($"Context '{name}' of type {typeof(TContext)} not found.");
    }
    
    public virtual TContext? TryGet<TContext>(string name)
    {
        if (Items.TryGetValue(name, out var context))
            return (TContext)context!;
        return default;
    }

    #endregion

}
