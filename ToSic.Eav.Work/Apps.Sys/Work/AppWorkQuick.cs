namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Helper to quickly get work done; should not be used for complex work or situations where shared context would be beneficial.
/// </summary>
/// <typeparam name="TWork"></typeparam>
/// <param name="appWorkCtxSvc"></param>
/// <param name="gen"></param>
public class AppWorkQuick<TWork>(AppWorkContextService appWorkCtxSvc, Generator<TWork, IAppWorkContext> gen)
    : ServiceBase("Wrk.Quick", connect: [])
    where TWork : IServiceWithSetup<IAppWorkContext>
{
    public TWork New(int appId, bool? showDrafts = default)
        => gen.New(appWorkCtxSvc.ContextNew(appId, showDrafts));
    
    public TWork New(IAppIdentity appIdentity, bool? showDrafts = default)
        => gen.New(appWorkCtxSvc.ContextNew(appIdentity, showDrafts));
    
    public TWork New(IAppReader appReader, bool? showDrafts = default)
        => gen.New(appWorkCtxSvc.ContextNew(appReader, showDrafts));
}
