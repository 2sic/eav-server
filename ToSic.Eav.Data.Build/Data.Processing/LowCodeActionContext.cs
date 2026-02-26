namespace ToSic.Eav.Data.Processing;

public record LowCodeActionContext
{
    //public Dictionary<Type, object> Unnamed { get; init; } = new();

    //public static Dictionary<Type, object> CreateUnnamed(params object[] contexts) =>
    //    contexts.ToDictionary(context => context.GetType(), context => context);
    //public Dictionary<Type, object> WithUnnamed(params object[] contexts)
    //{
    //    var copy = new Dictionary<Type, object>(Unnamed);
    //    foreach (var context in contexts) 
    //        copy[context.GetType()] = context;
    //    return copy;
    //}

    //public virtual TContext Get<TContext>()
    //{
    //    if (Unnamed.TryGetValue(typeof(TContext), out var context))
    //        return (TContext)context;

    //    throw new KeyNotFoundException($"Context of type {typeof(TContext)} not found.");
    //}

    //public virtual TContext? TryGet<TContext>()
    //{
    //    if (Unnamed.TryGetValue(typeof(TContext), out var context))
    //        return (TContext)context;

    //    return default;
    //}
    public Dictionary<string, object?> Named { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public virtual TContext Get<TContext>(string name)
    {
        if (Named.TryGetValue(name, out var context))
            return (TContext)context!;

        throw new KeyNotFoundException($"Context '{name}' of type {typeof(TContext)} not found.");
    }


}

public static class LowCodeActionContextExtensions
{
    public static LowCodeActionContext With<TContext>(this LowCodeActionContext context, string name, TContext? value) =>
        context with { Named = new(context.Named, StringComparer.OrdinalIgnoreCase)
            {
                [name] = value
            }
        };
}