using ToSic.Eav.WebApi.Sys.Context;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Sxc.Backend.Context;

public interface IUiContextParts : IServiceWithSetup<IAppReader?>
{
    ContextLanguageInfoDto? GetLanguage(Ctx flags);
    ContextResourceWithApp? GetSystem(Ctx flags);
    ContextResourceWithApp? GetSite(Ctx flags);
    WebResourceDto GetPage(Ctx flags);
    ContextEnableDto GetEnable(CtxEnable ctx);
    ContextAppDto? GetApp(Ctx flags);
    ContextUserDto? GetUser(Ctx flags);

    /// <summary>
    /// Features provided must be virtual, so that applications can override this.
    /// </summary>
    /// <returns></returns>
    IList<FeatureDto> GetFeatures(Ctx flags);
}