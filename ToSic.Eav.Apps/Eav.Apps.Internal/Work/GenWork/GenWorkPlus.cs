namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Helper to generate a App Work Unit class to get a single / simple thing done.
///
/// Primarily used to do one single operation such as a simple delete, publish etc.
/// </summary>
/// <typeparam name="TWork"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GenWorkPlus<TWork>(LazySvc<AppWorkContextService> ctxSvc, Generator<TWork> gen)
    : ServiceBase("App.WorkUn", connect: [ctxSvc, gen])
    where TWork : WorkUnitBase<IAppWorkCtxPlus>
{
    public AppWorkContextService CtxSvc => ctxSvc.Value;

    private TWork NewInternal(IAppWorkCtxPlus ctx)
    {
        var fresh = gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }

    public TWork New(IAppWorkCtxPlus ctx) => NewInternal(ctx);

    //public TWork New(AppState state, bool? showDrafts = default) => NewInternal(_ctxSvc.Value.ContextPlus(state, showDrafts: showDrafts));
    public TWork New(IAppReader state, bool? showDrafts = default)
        => NewInternal(ctxSvc.Value.ContextPlus(state, showDrafts: showDrafts));

    public TWork New(IAppIdentity identity, bool? showDrafts = default)
        => NewInternal(ctxSvc.Value.ContextPlus(identity, showDrafts: showDrafts));

    public TWork New(int appId, bool? showDrafts = default)
        => NewInternal(ctxSvc.Value.ContextPlus(appId, showDrafts: showDrafts));
}