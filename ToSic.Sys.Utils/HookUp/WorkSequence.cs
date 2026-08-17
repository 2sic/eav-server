namespace ToSic.Sys.HookUp;

/// <summary>
/// Call multiple works, which are registered in DI by type.
/// WARNING: not yet used / tested
/// </summary>
/// <typeparam name="TWork"></typeparam>
/// <typeparam name="TData"></typeparam>
/// <param name="works"></param>
public class WorkSequence<TWork, TData>(IEnumerable<TWork> works) : ServiceBase("Sec.Process")
    where TWork : class, IWork<TData, TData>
{
    public async Task<Package<TData>> Handle(WorkContext context, Package<TData> package)
    {
        var workList = works.ToList();
        var l = Log.Fn<Package<TData>>($"For {workList.Count} works");
        var payload = package.RePackage(package.Data);
        if (!workList.Any())
            return l.Return(payload, "no work found");

        foreach (var work in workList)
        {
            try
            {
                payload = await work.Handle(new(), payload);
            }
            catch (Exception ex)
            {
                l.Ex(message: $"Error running processor for work {work.GetType().Name}", ex);
            }
        }

        return l.Return(payload);
    }
}