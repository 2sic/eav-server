using ToSic.Lib.Coding;
using ToSic.Lib.DI;
// ReSharper disable RedundantAccessorBody

namespace ToSic.Lib.Services;

public abstract class ServiceWithOptionsBase<TService, TOptions>(string logName, Generator<TService> selfGenerator, NoParamOrder protect = default, object[] connect = default) :
    ServiceBase(logName, protect, connect: [selfGenerator, ..connect ?? []]),
    IServiceWithOptions<TService, TOptions>
    where TService : ServiceWithOptionsBase<TService, TOptions>, IServiceWithOptions<TService, TOptions>
    where TOptions : class, new()
{

    public virtual TOptions Options
    {
        get => field ??= new();
        private set => field = value;
    }

    protected bool OptionsAreDefault = true;

    //protected virtual TOptions GetDefault() => new();

    public TService New(TOptions options)
    {
        var instance = selfGenerator.New();
        instance.Options = options;
        instance.OptionsAreDefault = options == null;
        return instance;
    }
}