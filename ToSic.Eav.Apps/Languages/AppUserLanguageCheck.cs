using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.DI;
using static System.StringComparison;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.Apps.Languages
{
    public class AppUserLanguageCheck: ServiceBase
    {
        public AppUserLanguageCheck(LazyInit<IZoneMapper> zoneMapperLazy, IContextOfSite ctx, Generator<AppPermissionCheck> checkGenerator, LazyInit<IAppStates> appStatesLazy,
            LazyInit<IFeaturesInternal> featuresLazy)
            : base($"{LogNames.Eav}.LngChk") =>
            ConnectServices(
                _zoneMapperLazy = zoneMapperLazy,
                _ctx = ctx,
                _checkGenerator = checkGenerator,
                _appStatesLazy = appStatesLazy,
                _featuresLazy = featuresLazy
            );

        private readonly LazyInit<IZoneMapper> _zoneMapperLazy;
        private readonly IContextOfSite _ctx;
        private readonly Generator<AppPermissionCheck> _checkGenerator;
        private readonly LazyInit<IAppStates> _appStatesLazy;
        private readonly LazyInit<IFeaturesInternal> _featuresLazy;

        /// <summary>
        /// Test if the current user has explicit language editing permissions.
        /// </summary>
        /// <param name="appStateOrNull"></param>
        /// <returns>true in most admin-cases, false if feature enabled AND permissions configured AND not allowed</returns>
        public bool? UserRestrictedByLanguagePermissions(AppState appStateOrNull)
        {
            var wrapLog = Log.Fn<bool?>($"{appStateOrNull?.Name}({appStateOrNull?.AppId})");

            // Note: it's important that all cases where we don't detect a forbidden
            // we return null, and DON'T access _ctx.UserMayEdit, as it will recurse to here again
            if (!_featuresLazy.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage)) 
                return wrapLog.ReturnNull("feat disabled");

            // Check if we have any language rules
            var languages = LanguagesWithPermissions(appStateOrNull);
            if (languages == null || !languages.Any()) return wrapLog.ReturnNull("no config");

            // Check rules on current language
            var currentCode = _ctx.Site.CurrentCultureCode;
            var currentLang = languages.FirstOrDefault(lp => lp.Code.Equals(currentCode, InvariantCultureIgnoreCase));
            return wrapLog.Return(currentLang?.IsAllowed, $"permission: {currentLang?.IsAllowed}");
        }

        public List<AppUserLanguageState> LanguagesWithPermissions(AppState appStateOrNull)
        {
            var wrapLog = Log.Fn<List<AppUserLanguageState>>();

            // to solves the issue with globals settings languages that can not be saved if 
            // app languages are different from languages in global app and because global
            // settings are in primary appid=1, zoneId=1 without portal site we just return empty list for it
            // in other cases we get the languages from the app state or from context (http headers)
            var zoneMapper = _zoneMapperLazy.Value;
            var site = appStateOrNull != null ? zoneMapper.SiteOfZone(appStateOrNull.ZoneId) : _ctx.Site;
            if (site == null) return wrapLog.Return(new List<AppUserLanguageState>(), "null site");
            
            var languages = zoneMapper.CulturesWithState(site);

            // Check if ML-Permissions-Feature is enabled, otherwise don't check detailed permissions
            var mlFeatureEnabled = _featuresLazy.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage);
            var allowAllLanguages = !mlFeatureEnabled || _ctx.User.IsSystemAdmin;

            if (allowAllLanguages || appStateOrNull == null)
            {
                var noAppResult = languages
                    .Select(l => new AppUserLanguageState(l, true, -1))
                    .ToList();
                return wrapLog.Return(noAppResult, $"no-app {noAppResult.Count}");
            }

            var set = GetLanguagePermissions(appStateOrNull, languages);
            Log.A($"Found {set.Count} sets");
            var hasPermissions = set.Any(s => s.Permissions.Any());

            // Find primary app, or stop if we're already there
            if (!hasPermissions && appStateOrNull.NameId != Constants.PrimaryAppGuid)
            {
                Log.A("No permissions, and not primary app - will try that");
                var primaryId = _appStatesLazy.Value.PrimaryAppId(appStateOrNull.ZoneId);
                var primaryApp = _appStatesLazy.Value.Get(primaryId);
                set = GetLanguagePermissions(primaryApp, languages);
                hasPermissions = set.Any(s => s.Permissions.Any());
            }

            var defaultAllowed = _ctx.User.IsSystemAdmin || !hasPermissions;
            Log.A($"HasPermissions: {hasPermissions}, Initial Allowed: {defaultAllowed}");

            var newSet = set.Select(s =>
            {
                var permissionEntities = s.Permissions.ToList();
                var ok = defaultAllowed;
                if (!ok)
                {
                    var pChecker = _checkGenerator.New();
                    var permissions = permissionEntities.Select(p => new Permission(p));
                    pChecker.ForCustom(_ctx, appStateOrNull, permissions, Log);
                    ok = pChecker.PermissionsAllow(GrantSets.WriteSomething);
                }

                return new
                {
                    s.Language,
                    Allowed = ok,
                    PermissionCount = permissionEntities.Count
                };
            });

            var result = newSet
                .Select(s => new AppUserLanguageState(s.Language, s.Allowed, s.PermissionCount))
                .ToList();
            return wrapLog.Return(result, $"ok {result.Count}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appStateOrNull">The AppState which could hold permissions - or null if the app isn't there yet (like adding new module)</param>
        /// <param name="languages"></param>
        /// <returns></returns>
        private static List<LanguagePermission> GetLanguagePermissions(AppState appStateOrNull, List<ISiteLanguageState> languages)
        {
            var set = languages.Select(l => new LanguagePermission
            {
                Permissions = appStateOrNull?.GetMetadata(TargetTypes.Dimension, l.Code?.ToLowerInvariant(), Permission.TypeName)
                    ?? Array.Empty<IEntity>(),
                Language = l
            }).ToList();
            return set;
        }

        private class LanguagePermission
        {
            public IEnumerable<IEntity> Permissions;
            public ISiteLanguageState Language;
        }
    }
}
