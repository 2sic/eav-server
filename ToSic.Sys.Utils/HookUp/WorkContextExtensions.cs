namespace ToSic.Sys.HookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class WorkContextExtensions
{
    public static WorkContext With<TContext>(this WorkContext context, string name, TContext? value) =>
        context with { Items = new(context.Items, StringComparer.OrdinalIgnoreCase)
            {
                [name] = value
            }
        };
    public static WorkContext With<TContext>(this WorkContext context, TContext? value) =>
        context with { Items = new(context.Items, StringComparer.OrdinalIgnoreCase)
            {
                [typeof(TContext).Name] = value
            }
        };
    
    public static WorkContext With(this WorkContext context, IEnumerable<KeyValuePair<string, object?>> values)
    {
        var dic = new Dictionary<string, object?>(context.Items, StringComparer.OrdinalIgnoreCase);
        foreach (var keyValuePair in values)
            dic[keyValuePair.Key] = keyValuePair.Value;
        return context with
        {
            Items = dic
        };
    }
    
    public static WorkContext With(this WorkContext context, Dictionary<string, object?> values)
        => context.With(values as IEnumerable<KeyValuePair<string, object?>>);
}