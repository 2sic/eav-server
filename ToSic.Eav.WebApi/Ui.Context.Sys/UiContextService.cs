using ToSic.Eav.Ui.Context.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
internal class UiContextService(Generator<IUiContextPartsService, IAppReader?> contextPartsGen)
    : ServiceBase("Eav.UiCtx", connect: [contextPartsGen]), IUiContextService
{
    public void Setup(IAppReader? options) => AppReader = options;
    
    private IAppReader? AppReader { get; set; }
    
    public IUiContextPartsService ContextPartsSvc => field ??= contextPartsGen.New(AppReader);


    /// <inheritdoc />
    public ContextDto Get(Ctx flags, CtxEnable enableFlags)
    {
        return new()
        {
            App = TryGet(flags.HasFlag(Ctx.AppBasic) || flags.HasFlag(Ctx.AppAdvanced), () => ContextPartsSvc.GetApp(flags)),
            Enable = TryGet(enableFlags != CtxEnable.None, () => ContextPartsSvc.GetEnable(enableFlags)),
            Language = TryGet(flags.HasFlag(Ctx.Language), () => ContextPartsSvc.GetLanguage(flags)),
            Page = TryGet(flags.HasFlag(Ctx.Page), () => ContextPartsSvc.GetPage(flags)),
            Site = TryGet(flags.HasFlag(Ctx.Site), () => ContextPartsSvc.GetSite(flags)),
            System = TryGet(flags.HasFlag(Ctx.System), () => ContextPartsSvc.GetSystem(flags)),
            User = TryGet(flags.HasFlag(Ctx.User), () => ContextPartsSvc.GetUser(flags)),
            Features = TryGet(flags.HasFlag(Ctx.Features) || flags.HasFlag(Ctx.FeaturesForSystemTypes), () => ContextPartsSvc.GetFeatures(flags))
        };

    }

    private T? TryGet<T>(bool condition, Func<T?> getter) where T : class
    {
        if (!condition)
            return null;
        try
        {
            return getter();
        }
        catch (Exception e)
        {
            Log.A($"Error while getting context part {typeof(T).Name}: {e.Message}");
            return null;
        }
    }

}
