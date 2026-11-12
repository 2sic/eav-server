using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sxc.Backend.Context;

namespace ToSic.Eav.WebApi.Sys.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class UiContextBuilder(Generator<IUiContextParts, IAppReader?> contextPartsGen)
    : ServiceBase("Eav.UiCtx", connect: [contextPartsGen]), IUiContextBuilder
{
    public void Setup(IAppReader? options) => AppReader = options;
    
    private IAppReader? AppReader { get; set; }
    
    public IUiContextParts ContextParts => field ??= contextPartsGen.New(AppReader);


    /// <inheritdoc />
    public ContextDto Get(Ctx flags, CtxEnable enableFlags)
    {
        // logic for activating each part
        // 1. either that switch is on
        // 2. or the null-check: all is on
        // 3. This also means if the switch is off, it's off
        return new()
        {
            App = TryGet(flags.HasFlag(Ctx.AppBasic) || flags.HasFlag(Ctx.AppAdvanced), () => ContextParts.GetApp(flags)),
            Enable = TryGet(enableFlags != CtxEnable.None, () => ContextParts.GetEnable(enableFlags)),
            Language = TryGet(flags.HasFlag(Ctx.Language), () => ContextParts.GetLanguage(flags)),
            Page = TryGet(flags.HasFlag(Ctx.Page), () => ContextParts.GetPage(flags)),
            Site = TryGet(flags.HasFlag(Ctx.Site), () => ContextParts.GetSite(flags)),
            System = TryGet(flags.HasFlag(Ctx.System), () => ContextParts.GetSystem(flags)),
            User = TryGet(flags.HasFlag(Ctx.User), () => ContextParts.GetUser(flags)),
            Features = TryGet(flags.HasFlag(Ctx.Features) || flags.HasFlag(Ctx.FeaturesForSystemTypes), () => ContextParts.GetFeatures(flags))
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
