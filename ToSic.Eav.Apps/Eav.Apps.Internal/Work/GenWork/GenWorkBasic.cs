using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Helper to generate a App Work Unit class to get a single / simple thing done.
///
/// Primarily used to do one single operation such as a simple delete, publish etc.
/// </summary>
/// <typeparam name="TWork"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GenWorkBasic<TWork>(LazySvc<AppWorkContextService> ctxSvc, Generator<TWork> gen)
    : ServiceBase("App.WorkUn", connect: [ctxSvc, gen])
    where TWork : WorkUnitBase<IAppWorkCtx>
{
    public AppWorkContextService CtxSvc => ctxSvc.Value;
    public TWork NewInternal(IAppWorkCtx ctx)
    {
        var fresh = gen.New();
        fresh._initCtx(ctx);
        return fresh;
    }


    public TWork New(IAppReader appState) => NewInternal(ctxSvc.Value.Context(appState));

    public TWork New(int appId) => NewInternal(ctxSvc.Value.Context(appId));

    // These signatures are not used ATM, but might be useful in the future
    //public TWork New(IAppWorkCtx ctx) => NewInternal(ctx);
    //public TWork New(IAppIdentity identity) => NewInternal(_ctxSvc.Value.Context(identity));
}