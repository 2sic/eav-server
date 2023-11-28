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
public class GenWorkBasic<TWork>: ServiceBase where TWork : WorkUnitBase<IAppWorkCtx>
{
    private readonly LazySvc<AppWorkContextService> _ctxSvc;
    private readonly Generator<TWork> _gen;

    public GenWorkBasic(LazySvc<AppWorkContextService> ctxSvc, Generator<TWork> gen): base("App.WorkUn")
    {
        ConnectServices(_ctxSvc = ctxSvc, _gen = gen);
    }

    public AppWorkContextService CtxSvc => _ctxSvc.Value;
    public TWork NewInternal(IAppWorkCtx ctx)
    {
        var fresh = _gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }

    public TWork New(IAppWorkCtx ctx) => NewInternal(ctx);

    public TWork New(AppState appState) => NewInternal(_ctxSvc.Value.Context(appState));

    public TWork New(IAppState appState) => NewInternal(_ctxSvc.Value.Context(appState.Internal().AppState));

    public TWork New(IAppIdentity identity) => NewInternal(_ctxSvc.Value.Context(identity));

    public TWork New(int appId) => NewInternal(_ctxSvc.Value.Context(appId));
}