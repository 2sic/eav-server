using ToSic.Lib.Coding;
using ToSic.Lib.DI;
using ToSic.Sys.Services;


namespace ToSic.Lib.Services;

public abstract class ServiceWithOptionsBase<TService, TMyServices, TOptions>(TMyServices services, string logName, Generator<TService> selfGenerator, NoParamOrder protect = default, object[]? connect = default) :
    ServiceBase<TMyServices>(services, logName, protect, connect: [selfGenerator, .. connect ?? []]),
    IServiceWithOptions<TService, TOptions>,
    IServiceWithOptionsToSetup<TOptions>
    where TService : class, IServiceWithOptions<TService, TOptions>
    where TOptions : class, new()
    where TMyServices : MyServicesBase
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions Options
    {
        get => field ??= GetDefaultOptions();
        private set;
    }
    void IServiceWithOptionsToSetup<TOptions>.SetOptions(TOptions options) => Options = options;


    /// <summary>
    /// Method to generate new / default options. You can override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    protected virtual TOptions GetDefaultOptions() => new();

    /// <inheritdoc />
    public TService SpawnNew(TOptions? options = default)
    {
        var instance = selfGenerator.New();
        (instance as IServiceWithOptionsToSetup<TOptions>)?.SetOptions(options);

        //if (instance is ServiceWithOptionsBase<TService, TMyServices, TOptions> settable)
        //    settable.Options = options!;
        return instance;
    }
}