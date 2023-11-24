using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Work;

/// <summary>
/// Helper to generate a App Work Unit class to get a single / simple thing done.
///
/// Primarily used to do one single operation such as a simple delete, publish etc.
/// </summary>
/// <typeparam name="TWork"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class GenWorkPlus<TWork>: ServiceBase where TWork : WorkUnitBase<IAppWorkCtxPlus>
{
    private readonly LazySvc<AppWorkContextService> _ctxSvc;
    private readonly Generator<TWork> _gen;

    public GenWorkPlus(LazySvc<AppWorkContextService> ctxSvc, Generator<TWork> gen): base("App.WorkUn")
    {
        ConnectServices(_ctxSvc = ctxSvc, _gen = gen);
    }

    public AppWorkContextService CtxSvc => _ctxSvc.Value;

    private TWork NewInternal(IAppWorkCtxPlus ctx)
    {
        var fresh = _gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }

    public TWork New(IAppWorkCtxPlus ctx) => NewInternal(ctx);

    public TWork New(AppState state, bool? showDrafts = default) => NewInternal(_ctxSvc.Value.ContextPlus(state, showDrafts: showDrafts));

    public TWork New(IAppIdentity identity) => NewInternal(_ctxSvc.Value.ContextPlus(identity));

    public TWork New(int appId, bool? showDrafts = default) => NewInternal(_ctxSvc.Value.ContextPlus(appId, showDrafts: showDrafts));
}