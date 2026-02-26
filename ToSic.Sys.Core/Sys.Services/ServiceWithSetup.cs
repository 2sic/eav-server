namespace ToSic.Sys.Services;

public abstract class ServiceWithSetup<TOptions>(string logName, NoParamOrder npo = default, object[]? connect = default)
    : ServiceBase(logName, npo, connect: connect),
        IHasOptions<TOptions>,
        IServiceWithSetup<TOptions>
        where TOptions : class, new()
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions MyOptions
    {
        get => field ??= GetDefaultOptions();
        private set;
    }

    public virtual void Setup(TOptions options)
        => MyOptions = options;

    /// <summary>
    /// Method to generate new / default options. You can override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    protected virtual TOptions GetDefaultOptions()
        => new();
}