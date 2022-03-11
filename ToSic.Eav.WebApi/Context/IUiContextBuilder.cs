using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Context
{
    public interface IUiContextBuilder
    {
        /// <summary>
        /// Initialize the context builder
        /// </summary>
        /// <returns></returns>
        IUiContextBuilder InitApp(AppState appState, ILog parentLog);

        /// <summary>
        /// Get the context based on the situation
        /// </summary>
        /// <returns></returns>
        ContextDto Get(Ctx flags, CtxEnable enableFlags);
    }
}
