using ToSic.Lib.Coding;
using ToSic.Lib.DI;


namespace ToSic.Lib.Services;

public abstract class ServiceWithOptionsBase<TService, TOptions>(string logName, Generator<TService> selfGenerator, NoParamOrder protect = default, object[]? connect = default) :
    ServiceBase(logName, protect, connect: [selfGenerator, ..connect ?? []]),
    IServiceWithOptions<TService, TOptions>
    where TService : class, IServiceWithOptions<TService, TOptions>
    where TOptions : class, new()
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions Options
    {
        get => field ??= GetDefaultOptions();
        private set;
    }

    /// <summary>
    /// Method to generate new / default options. You can override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    protected virtual TOptions GetDefaultOptions() => new();

    /// <inheritdoc />
    public TService New(TOptions? options = default)
    {
        var instance = selfGenerator.New();
        if (instance is ServiceWithOptionsBase<TService, TOptions> settable)
            settable.Options = options!;
        return instance;
    }
}