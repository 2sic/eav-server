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
    //public AppWorkContextService CtxSvc => ctxSvc.Value;
    private TWorkContext NewInternal(IAppWorkCtx ctx)
    {
        var fresh = gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }


    public TWorkContext New(IAppReader appReader)
        => NewInternal(ctxSvc.Value.Context(appReader));

    public TWorkContext New(int appId)
        => NewInternal(ctxSvc.Value.Context(appId));

    // These signatures are not used ATM, but might be useful in the future
    //public TWork New(IAppWorkCtx ctx) => NewInternal(ctx);
    //public TWork New(IAppIdentity identity) => NewInternal(_ctxSvc.Value.Context(identity));
}