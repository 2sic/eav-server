using ToSic.Lib.Helpers;

namespace ToSic.Eav.Context.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ContextResolver: ServiceBase
{
    #region Constructor / DI

    private readonly Generator<IContextOfSite> _contextOfSite;
    private readonly Generator<IContextOfApp> _contextOfApp;

    public ContextResolver(
        //LazySvc<AppIdResolver> appIdResolverLazy,
        Generator<IContextOfSite> contextOfSite,
        Generator<IContextOfApp> contextOfApp) : this(contextOfSite, contextOfApp, "Eav.CtxRes")
    {
            
    }
    protected ContextResolver(Generator<IContextOfSite> contextOfSite,
        Generator<IContextOfApp> contextOfApp, string logName) : base(logName)
    {
        ConnectServices(
            _contextOfSite = contextOfSite,
            _contextOfApp = contextOfApp
        );
    }

    #endregion

    public IContextOfSite Site() => _site.Get(() => _contextOfSite.New());
    private readonly GetOnce<IContextOfSite> _site = new();


}