namespace ToSic.Eav.Data.Processing;

public record LowCodeActionContext
{
    public Dictionary<string, object?> Context { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public virtual TContext Get<TContext>(string name)
    {
        if (Context.TryGetValue(name, out var context))
            return (TContext)context!;

        throw new KeyNotFoundException($"Context '{name}' of type {typeof(TContext)} not found.");
    }


}