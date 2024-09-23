using ToSic.Eav.Apps.State;

namespace ToSic.Eav.WebApi.Context;

public interface IUiContextBuilder: IHasLog
{
    /// <summary>
    /// Initialize the context builder
    /// </summary>
    /// <returns></returns>
    IUiContextBuilder InitApp(IAppReader appState);

    /// <summary>
    /// Get the context based on the situation
    /// </summary>
    /// <returns></returns>
    ContextDto Get(Ctx flags, CtxEnable enableFlags);
}