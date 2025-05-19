using ToSic.Lib.Coding;


namespace ToSic.Lib.Services;

public abstract class ServiceWithSetup<TOptions>(string logName, NoParamOrder protect = default, object[]? connect = default)
    : ServiceBase(logName, protect, connect: connect),
        IHasOptions<TOptions>,
        IServiceWithSetup<TOptions>
        where TOptions : class, new()
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions Options
    {
        get => field ??= GetDefaultOptions();
        private set;
    }

    public virtual void Setup(TOptions options)
        => Options = options;

    /// <summary>
    /// Method to generate new / default options. You can override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    protected virtual TOptions GetDefaultOptions()
        => new();
}