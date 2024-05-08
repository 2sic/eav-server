using ToSic.Eav.Apps.State;
using ToSic.Eav.Context;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Admin.Metadata;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Admin;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppInternalsControllerReal(
    LazySvc<IContextOfSite> context,
    LazySvc<ContentTypeDtoService> ctApiLazy,
    LazySvc<IAppStates> appStates,
    LazySvc<EntityApi> entityApi,
    LazySvc<MetadataControllerReal> metadataControllerReal)
    : ServiceBase("Api.AppInternalsRl", connect: [context, ctApiLazy, appStates, entityApi, metadataControllerReal]),
        IAppInternalsController
{
    public const string LogSuffix = "AppInternals";

    /// <inheritdoc/>
    public AppInternalsDto Get(int appId, int targetType, string keyType, string key)
    {
        var systemConfiguration = TypeListInternal(appId, Scopes.SystemConfiguration).ToList();
        var settingsCustomExists = systemConfiguration.Any(ct => ct.Name == "SettingsCustom");
        var resourcesCustomExists = systemConfiguration.Any(ct => ct.Name == "ResourcesCustom");

        var appState = appStates.Value.GetReader(appId);
        var isGlobal = appState.IsGlobalSettingsApp();
        var isPrimary = appState.AppId == appStates.Value.PrimaryAppId(appState.ZoneId);

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
                        ? settingsCustomExists ? EntityListInternal(appId, "SettingsCustom", excludeAncestor: true) : null 
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
                        ? resourcesCustomExists ? EntityListInternal(appId, "ResourcesCustom", excludeAncestor: true) : null
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
                        ? settingsCustomExists ? FieldAllInternal(appId, "SettingsCustom") : null
                        : FieldAllInternal(appId, AppLoadConstants.TypeAppSettings)
                },

                // 7. .../api/2sxc/admin/field/all?appid=999&staticName=App-Resources
                {
                    "AppResources",
                    isGlobalOrPrimary
                        ? resourcesCustomExists ? FieldAllInternal(appId, "ResourcesCustom") : null
                        : FieldAllInternal(appId, AppLoadConstants.TypeAppResources)
                }
            },

            // 9. .../api/2sxc/admin/metadata/get?appId=999&targetType=3&keyType=number&key=999
            MetadataList = MetadataListInternal(appId, 3, "number", appId.ToString())
        };
    }

    private IEnumerable<ContentTypeDto> TypeListInternal(int appId, string scope = null, bool withStatistics = false)
        => ctApiLazy.Value/*.Init(appId)*/.List(appId, scope, withStatistics);

    private IEnumerable<Dictionary<string, object>> EntityListInternal(int appId, string contentType, bool excludeAncestor = true)
        => entityApi.Value.InitOrThrowBasedOnGrants(context.Value, appStates.Value.IdentityOfApp(appId), contentType, GrantSets.ReadSomething)
            .GetEntitiesForAdmin(contentType, excludeAncestor);

    private IEnumerable<ContentTypeFieldDto> FieldAllInternal(int appId, string typeName)
        => ctApiLazy.Value/*.Init(appId)*/.GetFields(appId, typeName);

    private MetadataListDto MetadataListInternal(int appId, int targetType, string keyType, string key, string contentType = null)
        => metadataControllerReal.Value.Get(appId, targetType, keyType, key, contentType);
}