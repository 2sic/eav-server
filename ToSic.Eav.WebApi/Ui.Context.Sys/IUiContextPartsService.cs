using ToSic.Eav.Ui.Context.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys;

/// <summary>
/// Mini-service to provide the parts of the context.
/// </summary>
/// <remarks>
/// Will be called by the <see cref="IUiContextService"/> to retrieve the parts.
/// Typically, there will be a layered implementation, beginning with the <see cref="EavUiContextPartsService"/>
/// but then there will also be a `SxcUiContextPartsService` and a `DnnUiContextPartsService`
/// which will add more information to the context, depending on the platform.
///
/// Important: as we want composition over inheritance,
/// we strongly recommend to implement this interface in a new class,
/// and not inherit from the `EavUiContextPartsService` class.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IUiContextPartsService : IServiceWithSetup<IAppReader?>
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