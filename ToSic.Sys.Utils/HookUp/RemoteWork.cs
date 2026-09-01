using ToSic.Sys.DI;

namespace ToSic.Sys.HookUp;

public record DoNamedInput<TData, TResult>(string Action, TData Input, TResult Fallback);
public record DoNamedInput<TData>(string Action, TData Input) : DoNamedInput<TData, TData>(Action, Input, Input);

/// <summary>
/// Call another work, which is registered in DI by name.
/// </summary>
/// <typeparam name="TWork"></typeparam>
/// <typeparam name="TData"></typeparam>
/// <typeparam name="TResult"></typeparam>
/// <param name="generator"></param>
public class RemoteWork<TWork, TData, TResult>(Generator<TWork> generator) : ServiceBase("Sec.Process")
    where TWork: class, IWork<TData, TResult>
{
    public async Task<Package<TResult>> Handle(WorkContext context, Package<DoNamedInput<TData, TResult>> package)
    {
        var l = Log.Fn<Package<TResult>>();
        var service = generator.TryNew(package.Data.Action);
        if (service == null)
            return l.Return(package.RePackage(package.Data.Fallback), "service not found");
        
        var result = await service.Handle(context, package.RePackage(package.Data.Input));
        return l.Return(result, "service handled");
    }
}