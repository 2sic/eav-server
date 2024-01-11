using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Helper to generate a App Work Unit class to get a single / simple thing done.
///
/// Primarily used to do one single operation such as a simple delete, publish etc.
/// </summary>
/// <typeparam name="TWork"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class GenWorkDb<TWork>: ServiceBase where TWork : WorkUnitBase<IAppWorkCtxWithDb>
{
    private readonly LazySvc<AppWorkContextService> _ctxSvc;
    private readonly Generator<TWork> _gen;

    public GenWorkDb(LazySvc<AppWorkContextService> ctxSvc, Generator<TWork> gen): base("App.WorkUn")
    {
        ConnectServices(_ctxSvc = ctxSvc, _gen = gen);
    }

    public AppWorkContextService CtxSvc => _ctxSvc.Value;

    private TWork NewInternal(IAppWorkCtxWithDb ctx)
    {
        var fresh = _gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }

    public TWork New(IAppWorkCtxWithDb ctx) => NewInternal(ctx);

    //public TWork New(AppState appState) => NewInternal(CtxSvc.CtxWithDb(appState));
    public TWork New(IAppStateInternal appState) => NewInternal(CtxSvc.CtxWithDb(appState));

    public TWork New(IAppIdentity identity) => NewInternal(_ctxSvc.Value.CtxWithDb(_ctxSvc.Value.AppStates.GetReader(identity)));

    public TWork New(int appId) => NewInternal(_ctxSvc.Value.CtxWithDb(_ctxSvc.Value.AppStates.GetReader(appId)));
}