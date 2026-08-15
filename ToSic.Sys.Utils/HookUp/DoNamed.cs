using ToSic.Sys.DI;

namespace ToSic.Sys.HookUp;

public class DoNamed<TWork, TData>(Generator<TWork> generator) : ServiceBase("Sec.Process")
    where TWork: class, IWork<TData, TData>
{
    public async Task<Package<TData>> Handle(WorkContext context, Package<(string Action, TData Data)> package)
    {
        var service = generator.TryNew(package.Data.Action);
        if (service == null)
            return package.RePackage(package.Data.Data);
        
        var result = await service.Handle(context, package.RePackage(package.Data.Data));
        return result;
    }
}