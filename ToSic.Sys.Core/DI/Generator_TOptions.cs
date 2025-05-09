namespace ToSic.Lib.DI;

internal class Generator<TService, TOptions>(IServiceProvider sp)
    : Generator<TService>(sp), IGenerator<TService, TOptions>
    where TService : class, INeedsOptions<TOptions>
    where TOptions : class
{
    public TService New(TOptions options)
    {
        var instance = base.New();
        instance.Options = options;
        return instance;
    }

    [Obsolete("Do not use this, always use the one with options - will throw an error")]
    public new TService New()
        => throw new InvalidOperationException($"You must call {nameof(New)} with options first.");
}