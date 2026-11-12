using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Context;

public interface IUiContextBuilder: IServiceWithSetup<IAppReader?>
{
    /// <summary>
    /// Get the context based on the situation
    /// </summary>
    /// <returns></returns>
    ContextDto Get(Ctx flags, CtxEnable enableFlags);
}