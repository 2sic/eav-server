namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Helper to generate a App Work Unit class to get a single / simple thing done.
///
/// Primarily used to do one single operation such as a simple delete, publish etc.
/// </summary>
/// <typeparam name="TWork"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GenWorkDb<TWork>(LazySvc<AppWorkContextService> ctxSvc, Generator<TWork> gen)
    : ServiceBase("App.WorkUn", connect: [ctxSvc, gen])
    where TWork : WorkUnitBase<IAppWorkCtxWithDb>
{
    public AppWorkContextService CtxSvc => ctxSvc.Value;

    private TWork NewInternal(IAppWorkCtxWithDb ctx)
    {
        var fresh = gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }

    public TWork New(IAppWorkCtxWithDb ctx)
        => NewInternal(ctx);

    public TWork New(IAppReader appReader)
        => NewInternal(CtxSvc.CtxWithDb(appReader));

    public TWork New(IAppIdentity identity)
        => NewInternal(ctxSvc.Value.CtxWithDb(ctxSvc.Value.AppReaders.Get(identity)));

    public TWork New(int appId)
        => NewInternal(ctxSvc.Value.CtxWithDb(ctxSvc.Value.AppReaders.Get(appId)));
}