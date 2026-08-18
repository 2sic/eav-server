namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Helper to generate a App Work Unit class to get a single / simple thing done.
///
/// Primarily used to do one single operation such as a simple delete, publish etc.
/// </summary>
/// <typeparam name="TWorkContext"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GenWorkBasic<TWorkContext>(LazySvc<AppWorkContextService> ctxSvc, Generator<TWorkContext> gen)
    : ServiceBase("App.WorkUn", connect: [ctxSvc, gen])
    where TWorkContext : WorkUnitBase<IAppWorkCtx>
{
    public TWorkContext New(int appId)
        => NewInternal(ctxSvc.Value.Context(appId));

    private TWorkContext NewInternal(IAppWorkCtx ctx)
    {
        var fresh = gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }

}