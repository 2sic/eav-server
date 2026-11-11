using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Context;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Cms;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sxc.Backend.Context;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Sys.Context;

public class UiContextPartsEav(
    IContextOfSiteService siteCtxSvc,
    LazySvc<ISysFeaturesService> features,
    LazySvc<IUiData> uiDataLazy,
    LazySvc<ContextLanguageDtoService> languagesBackend,
    IAppPathsMicroSvc appPaths,
    IAppsCatalog appsCatalog)
    : ServiceWithSetup<IAppReader?>("Sxc.CtxPrt", connect: [siteCtxSvc, languagesBackend, features, uiDataLazy, appPaths, appsCatalog]), IUiContextParts
{
    private ISite Site => field ??= siteCtxSvc.Context.Site;
    protected int ZoneId => siteCtxSvc.Context.Site.ZoneId;

    public ContextLanguageInfoDto? GetLanguage(Ctx flags)
    {
        if (ZoneId == 0)
            return null;
        var site = Site;

        var converted = languagesBackend.Value.GetLanguagesOfApp(MyOptions);

        return new()
        {
            Current = site.CurrentCultureCode,
            Primary = converted.Any() ? site.DefaultCultureCode : site.CurrentCultureCode, // in special case when no languages are available, use the current culture to fix translation issue in UI
            List = converted,
        };
    }

    public ContextResourceWithApp? GetSystem(Ctx flags)
    {
        var result = new ContextResourceWithApp
        {
            Url = "/"
        };
        // Stop now if we don't need advanced infos
        if (!flags.HasFlag(Ctx.AppAdvanced))
            return result;

        // Otherwise also add the global app id
        return result with { PrimaryApp = new(1, 1), DefaultApp = new(1, 1) };
    }

    public ContextResourceWithApp? GetSite(Ctx flags)
    {
        var result = new ContextResourceWithApp
        {
            Id = Site.Id,
            Url = "//" + Site.UrlRoot + "/",
        };
        // Stop now if we don't need advanced infos
        if (!flags.HasFlag(Ctx.AppAdvanced))
            return result;

        // Otherwise also add the global appId
        var zoneId = Site.ZoneId;
        return result with
        {
            DefaultApp = appsCatalog.DefaultAppIdentity(zoneId).PureIdentityExplicit(),
            PrimaryApp = appsCatalog.PrimaryAppIdentity(zoneId).PureIdentityExplicit()
        };
    }

    public WebResourceDto GetPage(Ctx flags) =>
        new()
        {
            Id = EavConstants.NullId,
        };

    public ContextEnableDto GetEnable(CtxEnable ctx)
    {
        var isRealApp = MyOptions?.Specs != null && !MyOptions.Specs.IsContentApp();
        var user = siteCtxSvc.Context.User;
        return new()
        {
            DebugMode = user.IsSystemAdmin ||
                        features.Value.IsEnabled(BuiltInFeatures.EditUiAllowDebugModeForEditors),
            AppPermissions = ctx.HasFlag(CtxEnable.AppPermissions) ? isRealApp : null,
            CodeEditor = ctx.HasFlag(CtxEnable.CodeEditor) ? user.IsSystemAdmin : null,
            Query = ctx.HasFlag(CtxEnable.Query) ? isRealApp && user.IsSystemAdmin : null,
            FormulaSave = ctx.HasFlag(CtxEnable.FormulaSave) ? user.IsSystemAdmin : null,
            OverrideEditRestrictions = ctx.HasFlag(CtxEnable.OverrideEditRestrictions) ? user.IsSystemAdmin : null,
        };
    }

    public string GetGettingStartedUrl() => EavConstants.UrlNotInitialized;

    public ContextAppDto? GetApp(Ctx flags)
    {
        if (MyOptions == null)
            return null;
        var appReader = MyOptions;
        var appSpecs = appReader.Specs;
        var paths = appPaths.Get(appReader, Site);
        var result = new ContextAppDto
        {
            Id = appSpecs.AppId,
            Name = appSpecs.Name,
            Folder = appSpecs.Folder,
            Url = paths?.Path,
            SharedUrl = paths?.PathShared
        };

        // Stop now if we don't need edit or advanced
        if (!flags.HasFlag(Ctx.AppEdit) && !flags.HasFlag(Ctx.AppAdvanced))
            return result;

        var isGlobal = appSpecs.IsGlobalSettingsApp();
        result = result with
        {
            IsGlobalApp = isGlobal,
            IsSiteApp = appSpecs.IsSiteSettingsApp(),
            // only check content if not global, as that has the same id
            IsContentApp = !isGlobal && appSpecs.IsContentApp()
        };

        // Stop now if we don't need advanced infos
        if (!flags.HasFlag(Ctx.AppAdvanced))
            return result;

        result = result with
        {
            GettingStartedUrl = GetGettingStartedUrl(),
            Identifier = appSpecs.NameId,
            // #SiteApp v13
            SettingsScope = isGlobal
                ? "Global"
                : appSpecs.IsSiteSettingsApp()
                    ? "Site"
                    : "App"
        };

        return result with
        {
            Permissions = new() { Count = appReader.Specs.Metadata.Permissions.Count() },
            IsShared = appReader.IsShared(),
            IsInherited = appReader.IsInherited()
        };

    }

    public ContextUserDto? GetUser(Ctx flags)
    {
        var user = siteCtxSvc.Context.User;
        var userDto = new ContextUserDto
        {
            Email = user.Email,
            Id = user.Id,
            Guid = user.Guid,
            IsSystemAdmin = user.IsSystemAdmin,
            IsAnonymous = user.IsAnonymous,
            IsSiteAdmin = user.IsSiteAdmin,
            IsContentEditor = user.IsContentEditor,
            IsContentAdmin = user.IsContentAdmin,
            Name = user.Name,
            Username = user.Username,
            Roles = [], // flags.HasFlag(Ctx.UserRoles) ? usersSvc.Value.GetCurrentUser().Roles.Select(r => r.Name).ToListOpt() : [],
        };
        return userDto;
    }

    /// <summary>
    /// Features provided must be virtual, so that applications can override this.
    /// </summary>
    /// <returns></returns>
    public IList<FeatureDto> GetFeatures(Ctx flags)
        => uiDataLazy.Value.FeaturesDto(siteCtxSvc.Context.Permissions.IsContentAdmin, flags.HasFlag(Ctx.FeaturesForSystemTypes));
}