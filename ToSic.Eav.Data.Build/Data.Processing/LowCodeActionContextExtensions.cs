namespace ToSic.Eav.Data.Processing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LowCodeActionContextExtensions
{
    public static LowCodeActionContext With<TContext>(this LowCodeActionContext context, string name, TContext? value) =>
        context with { Context = new(context.Context, StringComparer.OrdinalIgnoreCase)
            {
                [name] = value
            }
        };
}