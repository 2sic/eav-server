using ToSic.Lib.Coding;
using ToSic.Lib.DI;

namespace ToSic.Lib.Services;

public abstract class ServiceRespawnBase<TService, TOptions>(string logName, Generator<TService> selfGenerator, NoParamOrder protect = default, object[]? connect = default) :
    ServiceBase(logName, protect, connect: [selfGenerator, ..connect ?? []]),
    IServiceRespawn<TService, TOptions>,
    IServiceWithSetup<TOptions>
    where TService : class, IServiceRespawn<TService, TOptions>
    where TOptions : class, new()
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions Options
    {
        get => field ??= GetDefaultOptions();
        set;
    }

    void IServiceWithSetup<TOptions>.Setup(TOptions options)
        => Options = options;

    /// <summary>
    /// Method to generate new / default options. You can override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    protected virtual TOptions GetDefaultOptions() => new();

    /// <inheritdoc />
    public TService SpawnNew(TOptions? options = default)
    {
        var instance = selfGenerator.New();
        (instance as IServiceWithSetup<TOptions>)?.Setup(options);
        return instance;
    }
}