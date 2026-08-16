using ToSic.Sys.DI;

namespace ToSic.Sys.HookUp;

public interface IHookUp
{
    IHookUpWork<TData> StartWith<TData>(TData data);
}

internal class HookUpBase(IServiceProvider serviceProvider): ServiceBase("Hup.HookUp"), IHookUp
{
    public IHookUpWork<TData> StartWith<TData>(TData data)
        => new HookUp<TData>(null, data.ToPackage(), serviceProvider);
}

public interface IHookUpWork
{
}

public interface IHookUpWork<TData>: IHookUpWork, IHasLog
{
    internal IServiceProvider ServiceProvider { get; }
    public Package<TData> Package { get; }
    internal WorkContext Context { get; }

    // TODO: STILL ON THE INTERFACE, BUT TRYING TO MOVE TO EXTENSION METHOD

    Task<IHookUpWork<TOutput>> Work<TWork, TOutput>()
        where TWork : IWork<TData, TOutput>;

    Task<IHookUpWork<TData>> Work<TWork>()
        where TWork : IWork<TData, TData>;

    //Task<IHookUpWork<TOutput>> Work<TOutput>(IWork<TData, TOutput> work);
}

internal class HookUp<TData>(WorkContext? context, Package<TData>  package, IServiceProvider serviceProvider) : ServiceBase("Hup.HookUp"), IHookUpWork<TData>
{
    public IServiceProvider ServiceProvider => serviceProvider;
    public Package<TData> Package => package;

    public WorkContext Context
    {
        get => field ??= context == null
            ? new() { HookUp = this }
            : context with { HookUp = this };
        init;
    }

   
    public async Task<IHookUpWork<TOutput>> Work<TWork, TOutput>()
        where TWork : IWork<TData, TOutput> =>
        await this.Work(serviceProvider.Build<TWork>());

    public Task<IHookUpWork<TData>> Work<TWork>()
        where TWork : IWork<TData, TData>
        => Work<TWork, TData>();
}

public static class HookUpExtensions
{
    public static async Task<IHookUpWork<TOutput>> Work<TData, TOutput>(this IHookUpWork<TData> hookUp, IWork<TData, TOutput> work)
    {
        var result = await work.Handle(hookUp.Context, hookUp.Package);
        var chain = new HookUp<TOutput>(hookUp.Context, result, hookUp.ServiceProvider);
        return chain;
    }

    public static Task<IHookUpWork<TData>> Work<TData, TWork>(this IHookUpWork<TData> hookUp)
        where TWork : IWork<TData, TData>
        => hookUp.Work(hookUp.ServiceProvider.Build<TWork>());

    //    public static async Task<IHookUpWork<TOutput>> Work<TData, TWork, TOutput>(this IHookUpWork<TData> hookUpWork)
    //        where TWork: IWork<TData, TOutput>
    //    {
    //        var service = hookUpWork.ServiceProvider.Build<TWork>();
    //        var result = await service.Handle(hookUpWork.Context, hookUpWork.Package);
    //        var chain = new HookUp<TOutput>(hookUpWork.Context, result, hookUpWork.ServiceProvider);
    //        return chain;
    //    }

}