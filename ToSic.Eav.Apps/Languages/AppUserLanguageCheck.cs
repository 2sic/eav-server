using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Apps.Languages
{
    public class AppUserLanguageCheck: HasLog<AppUserLanguageCheck>
    {
        public AppUserLanguageCheck(Lazy<IZoneMapper> zoneMapperLazy, IContextOfSite ctx, IServiceProvider checkGenerator, Lazy<IAppStates> appStatesLazy)
            : base($"{LogNames.Eav}.LngChk")
        {
            _zoneMapperLazy = zoneMapperLazy;
            _ctx = ctx;
            _checkGenerator = checkGenerator;
            _appStatesLazy = appStatesLazy;
        }
        private readonly Lazy<IZoneMapper> _zoneMapperLazy;
        private readonly IContextOfSite _ctx;
        private readonly IServiceProvider _checkGenerator;
        private readonly Lazy<IAppStates> _appStatesLazy;

        public List<AppUserLanguageState> LanguagesWithPermissions(AppState appStateOrNull)
        {
            var wrapLog = Log.Call<List<AppUserLanguageState>>();

            var languages = _zoneMapperLazy.Value.CulturesWithState(_ctx.Site);

            if (appStateOrNull == null)
            {
                var noAppResult = languages
                    .Select(l => new AppUserLanguageState(l, true, -1))
                    .ToList();
                return wrapLog($"no-app {noAppResult.Count}", noAppResult);
            }

            var set = GetLanguagePermissions(appStateOrNull, languages);
            Log.Add($"Found {set.Count} sets");
            var hasPermissions = set.Any(s => s.Permissions.Any());

            // Find primary app, or stop if we're already there
            if (!hasPermissions && appStateOrNull.NameId != Constants.PrimaryAppGuid)
            {
                Log.Add("No permissions, and not primary app - will try that");
                var primaryId = _appStatesLazy.Value.PrimaryAppId(appStateOrNull.ZoneId);
                var primaryApp = _appStatesLazy.Value.Get(primaryId);
                set = GetLanguagePermissions(primaryApp, languages);
                hasPermissions = set.Any(s => s.Permissions.Any());
            }

            var defaultAllowed = _ctx.User.IsSuperUser || !hasPermissions;
            Log.Add($"HasPermissions: {hasPermissions}, Initial Allowed: {defaultAllowed}");

            var newSet = set.Select(s =>
            {
                var permissionEntities = s.Permissions.ToList();
                var ok = defaultAllowed;
                if (!ok)
                {
                    var pChecker = _checkGenerator.Build<AppPermissionCheck>();
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
            return wrapLog($"ok {result.Count}", result);
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
                Language = l,
                Allowed = true,
            }).ToList();
            return set;
        }

        private class LanguagePermission
        {
            public IEnumerable<IEntity> Permissions;
            public bool Allowed;
            public ISiteLanguageState Language;
        }
    }
}
