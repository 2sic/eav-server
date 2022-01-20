﻿using System;
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

        public List<AppUserLanguageState> LanguagesWithPermissions(AppState appState)
        {
            var wrapLog = Log.Call<List<AppUserLanguageState>>();

            var languages = _zoneMapperLazy.Value.CulturesWithState(_ctx.Site);

            var set = GetLanguagePermissions(appState, languages);
            Log.Add($"Found {set.Count} sets");
            var hasPermissions = set.Any(s => s.Permissions.Any());

            // Find primary app, or stop if we're already there
            if (!hasPermissions && appState.AppGuidName != Constants.PrimaryAppGuid)
            {
                Log.Add("No permissions, and not primary app - will try that");
                var primaryId = _appStatesLazy.Value.PrimaryAppId(appState.ZoneId);
                var primaryApp = _appStatesLazy.Value.Get(primaryId);
                set = GetLanguagePermissions(primaryApp, languages);
                hasPermissions = set.Any(s => s.Permissions.Any());
            }

            var defaultAllowed = _ctx.User.IsSuperUser || !hasPermissions;
            Log.Add($"HasPermissions: {hasPermissions}, Initial Allowed: {defaultAllowed}");

            var newSet = set.Select(s =>
            {
                var ok = defaultAllowed;
                if (!ok)
                {
                    var pChecker = _checkGenerator.Build<AppPermissionCheck>();
                    var permissions = s.Permissions.Select(p => new Permission(p));
                    pChecker.ForCustom(_ctx, appState, permissions, Log);
                    ok = pChecker.PermissionsAllow(GrantSets.WriteSomething);
                }

                return new
                {
                    s.Language,
                    Allowed = ok,
                };
            });

            var result = newSet
                .Select(s => new AppUserLanguageState(s.Language.Code, s.Language.Culture, s.Language.IsEnabled, s.Allowed))
                .ToList();
            return wrapLog("ok", result);
        }

        private static List<LanguagePermission> GetLanguagePermissions(AppState appState, List<ISiteLanguageState> languages)
        {
            var set = languages.Select(l => new LanguagePermission
            {
                Permissions = appState.GetMetadata(TargetTypes.Dimension, l.Code?.ToLowerInvariant(), Permission.TypeName),
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
