using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.WebApi.Sys.Admin.Metadata;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Eav.WebApi.Sys.Entities;
using ToSic.Sys.Security.Permissions;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppInternalsControllerReal(
    LazySvc<IContextOfSite> context,
    LazySvc<ContentTypeDtoService> ctApiLazy,
    IAppsCatalog appsCatalog,
    LazySvc<IAppsCatalog> appCatalog,
    LazySvc<EntityApi> entityApi,
    LazySvc<MetadataControllerReal> metadataControllerReal)
    : ServiceBase("Api.AppInternalsRl", connect: [context, ctApiLazy, appsCatalog, appCatalog, entityApi, metadataControllerReal]),
        IAppInternalsController
{
    public const string LogSuffix = "AppInternals";

    /// <inheritdoc/>
    public AppInternalsDto Get(int appId)
    {
        var systemConfiguration = TypeListInternal(appId, ScopeConstants.SystemConfiguration).ToList();
        var settingsCustomExists = systemConfiguration.Any(ct => ct.Name == "SettingsCustom");
        var resourcesCustomExists = systemConfiguration.Any(ct => ct.Name == "ResourcesCustom");

        var appState = appCatalog.Value.AppIdentity(appId);
        var isGlobal = appState.IsGlobalSettingsApp();
        var isPrimary = appState.AppId == appsCatalog.PrimaryAppIdentity(appState.ZoneId).AppId;

        var isGlobalOrPrimary = isGlobal || isPrimary;

        return new()
        {
            // 1. .../api/2sxc/admin/type/list?appId=999&scope=System.Configuration
            //SystemConfiguration = systemConfiguration,

            EntityLists = new Dictionary<string, IEnumerable<IDictionary<string, object>>>
            {
                // 2. .../api/2sxc/admin/entity/list?appId=999&contentType=SettingsSystem
                {
                    AppStackConstants.Settings.SystemType,
                    EntityListInternal(appId, AppStackConstants.Settings.SystemType)
                },

                // 3. .../api/2sxc/admin/entity/list?appId=999&contentType=App-Settings
                {
                    "AppSettings",
                    isGlobalOrPrimary
                        ? settingsCustomExists
                            ? EntityListInternal(appId, "SettingsCustom", excludeAncestor: true)
                            : null 
                        : EntityListInternal(appId, AppLoadConstants.TypeAppSettings, excludeAncestor: true)
                },

                // 5. .../api/2sxc/admin/entity/list?appId=999&contentType=ResourcesSystem
                {
                    AppStackConstants.Resources.SystemType,
                    EntityListInternal(appId, AppStackConstants.Resources.SystemType)
                },

                // 6. .../api/2sxc/admin/entity/list?appId=999&contentType=App-Resources
                {
                    "AppResources",
                    isGlobalOrPrimary
                        ? resourcesCustomExists
                            ? EntityListInternal(appId, "ResourcesCustom", excludeAncestor: true)
                            : null
                        : EntityListInternal(appId, AppLoadConstants.TypeAppResources, excludeAncestor: true)

                },

                // 8. .../api/2sxc/admin/entity/list?appId=999&contentType=2SexyContent-App
                {
                    "ToSxcContentApp",
                    EntityListInternal(appId, AppLoadConstants.TypeAppConfig, excludeAncestor: true)
                }
            },

            FieldAll = new Dictionary<string, IEnumerable<ContentTypeFieldDto>>
            {
                // 4. .../api/2sxc/admin/field/all?appid=999&staticName=App-Settings
                {
                    "AppSettings",
                    isGlobalOrPrimary
                        ? settingsCustomExists
                            ? FieldAllInternal(appId, "SettingsCustom")
                            : null
                        : FieldAllInternal(appId, AppLoadConstants.TypeAppSettings)
                },

                // 7. .../api/2sxc/admin/field/all?appid=999&staticName=App-Resources
                {
                    "AppResources",
                    isGlobalOrPrimary
                        ? resourcesCustomExists
                            ? FieldAllInternal(appId, "ResourcesCustom")
                            : null
                        : FieldAllInternal(appId, AppLoadConstants.TypeAppResources)
                }
            },

            // 9. .../api/2sxc/admin/metadata/get?appId=999&targetType=3&keyType=number&key=999
            MetadataList = MetadataListInternal(appId, 3, "number", appId.ToString())
        };
    }

    private IList<ContentTypeDto> TypeListInternal(int appId, string scope = null, bool withStatistics = false)
        => ctApiLazy.Value.List(appId, scope, withStatistics);

    private List<Dictionary<string, object>> EntityListInternal(int appId, string contentType, bool excludeAncestor = true)
        => entityApi.Value.InitOrThrowBasedOnGrants(context.Value, appsCatalog.AppIdentity(appId), contentType, GrantSets.ReadSomething)
            .GetEntitiesForAdmin(contentType, excludeAncestor);

    private IEnumerable<ContentTypeFieldDto> FieldAllInternal(int appId, string typeName)
        => ctApiLazy.Value.GetFields(appId, typeName);

    private MetadataListDto MetadataListInternal(int appId, int targetType, string keyType, string key, string contentType = null)
        => metadataControllerReal.Value.Get(appId, targetType, keyType, key, contentType);
}