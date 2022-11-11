using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.DI;
using ToSic.Lib.Logging;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Admin.Metadata;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Admin
{
    public class AppInternalsControllerReal : HasLog, IAppInternalsController
    {
        public const string LogSuffix = "AppInternals";
        public AppInternalsControllerReal(
            LazyInitLog<IContextOfSite> context,
            Lazy<ContentTypeApi> ctApiLazy,
            Lazy<IAppStates> appStates,
            Lazy<EntityApi> entityApi,
            Lazy<MetadataControllerReal> metadataControllerReal)
            : base("Api.AppInternalsRl")
        {
            _context = context.SetLog(Log);
            _ctApiLazy = ctApiLazy;
            _appStates = appStates;
            _entityApi = entityApi;
            _metadataControllerReal = metadataControllerReal;
        }

        private readonly LazyInitLog<IContextOfSite> _context;
        private readonly Lazy<ContentTypeApi> _ctApiLazy;
        private readonly Lazy<IAppStates> _appStates;
        private readonly Lazy<EntityApi> _entityApi;
        private readonly Lazy<MetadataControllerReal> _metadataControllerReal;

        /// <inheritdoc/>
        public AppInternalsDto Get(int appId, int targetType, string keyType, string key)
        {
            var systemConfiguration = TypeListInternal(appId, Scopes.SystemConfiguration);
            var settingsCustomExists = systemConfiguration.Any(ct => ct.Name == "SettingsCustom");
            var resourcesCustomExists = systemConfiguration.Any(ct => ct.Name == "ResourcesCustom");

            var appState = _appStates.Value.Get(appId);
            var isGlobal = appState.IsGlobalSettingsApp();
            var isPrimary = appState.AppId == _appStates.Value.PrimaryAppId(appState.ZoneId);

            return new AppInternalsDto()
            {
                // 1. .../api/2sxc/admin/type/list?appId=999&scope=System.Configuration
                //SystemConfiguration = systemConfiguration,

                EntityLists = new Dictionary<string, IEnumerable<IDictionary<string, object>>>
                {
                    // 2. .../api/2sxc/admin/entity/list?appId=999&contentType=SettingsSystem
                    {
                        ConfigurationConstants.Settings.SystemType,
                        EntityListInternal(appId, ConfigurationConstants.Settings.SystemType)
                    },

                    // 3. .../api/2sxc/admin/entity/list?appId=999&contentType=App-Settings
                    {
                        "AppSettings",
                        (isGlobal || isPrimary) 
                            ? (settingsCustomExists ? EntityListInternal(appId, "SettingsCustom", excludeAncestor: true) : null) 
                            : EntityListInternal(appId, AppLoadConstants.TypeAppSettings, excludeAncestor: true)
                    },

                    // 5. .../api/2sxc/admin/entity/list?appId=999&contentType=ResourcesSystem
                    {
                        ConfigurationConstants.Resources.SystemType,
                        EntityListInternal(appId, ConfigurationConstants.Resources.SystemType)
                    },

                    // 6. .../api/2sxc/admin/entity/list?appId=999&contentType=App-Resources
                    {
                        "AppResources",
                        (isGlobal || isPrimary)
                            ? (resourcesCustomExists ? EntityListInternal(appId, "ResourcesCustom", excludeAncestor: true) : null)
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
                        (isGlobal || isPrimary)
                            ? (settingsCustomExists ? FieldAllInternal(appId, "SettingsCustom") : null)
                            : FieldAllInternal(appId, AppLoadConstants.TypeAppSettings)
                    },

                    // 7. .../api/2sxc/admin/field/all?appid=999&staticName=App-Resources
                    {
                        "AppResources",
                        (isGlobal || isPrimary)
                            ? (resourcesCustomExists ? FieldAllInternal(appId, "ResourcesCustom") : null)
                            : FieldAllInternal(appId, AppLoadConstants.TypeAppResources)
                    }
                },

                // 9. .../api/2sxc/admin/metadata/get?appId=999&targetType=3&keyType=number&key=999
                MetadataList = MetadataListInternal(appId, 3, "number", appId.ToString())
            };
        }

        private IEnumerable<ContentTypeDto> TypeListInternal(int appId, string scope = null, bool withStatistics = false)
            => _ctApiLazy.Value.Init(appId, Log).Get(scope, withStatistics);

        private IEnumerable<Dictionary<string, object>> EntityListInternal(int appId, string contentType, bool excludeAncestor = true)
            => _entityApi.Value.InitOrThrowBasedOnGrants(_context.Ready, _appStates.Value.Get(appId), contentType, GrantSets.ReadSomething, Log)
                .GetEntitiesForAdmin(contentType, excludeAncestor);

        private IEnumerable<ContentTypeFieldDto> FieldAllInternal(int appId, string staticName)
            => _ctApiLazy.Value.Init(appId, Log).GetFields(staticName);

        private MetadataListDto MetadataListInternal(int appId, int targetType, string keyType, string key, string contentType = null)
            => _metadataControllerReal.Value.Get(appId, targetType, keyType, key, contentType);
    }
}
