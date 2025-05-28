using ToSic.Eav.Apps;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Context.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContextResolver(
    Generator<IContextOfSite> siteCtxGenerator,
    Generator<IContextOfApp> appCtxGenerator,
    string logName = default,
    object[] connect = default)
    : ServiceBase(logName ?? "Eav.CtxRes", connect: [..connect ?? [], siteCtxGenerator, appCtxGenerator])
{
   
    public IContextOfSite Site() => _site.Get(siteCtxGenerator.New);
    private readonly GetOnce<IContextOfSite> _site = new();


    public IContextOfApp SetApp(IAppIdentity appIdentity)
    {
        var appCtx = appCtxGenerator.New();
        appCtx.ResetApp(appIdentity);
        AppContextFromAppOrBlock = appCtx;
        return appCtx;
    }

    public IContextOfApp AppRequired()
        => AppContextFromAppOrBlock ?? throw new($"To call {nameof(AppRequired)} first call {nameof(SetApp)}");

    public IContextOfApp AppOrNull() => AppContextFromAppOrBlock;

    /// <summary>
    /// This is set whenever an App Context or Block Context are set.
    /// </summary>
    protected IContextOfApp AppContextFromAppOrBlock { get; set; }
}