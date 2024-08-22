﻿using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Internal;
using ToSic.Eav.Security.Permissions;
using static System.StringComparison;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.Cms.Internal.Languages;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppUserLanguageCheck(
    LazySvc<IZoneMapper> zoneMapperLazy,
    IContextOfSite ctx,
    Generator<AppPermissionCheck> checkGenerator,
    LazySvc<IAppReaders> appReadersLazy,
    LazySvc<IEavFeaturesService> featuresLazy)
    : ServiceBase($"{EavLogs.Eav}.LngChk", connect: [zoneMapperLazy, ctx, checkGenerator, appReadersLazy, featuresLazy])
{
    /// <summary>
    /// Test if the current user has explicit language editing permissions
    /// on ONE App.
    /// </summary>
    /// <param name="appReader"></param>
    /// <returns>true in most admin-cases, false if feature enabled AND permissions configured AND not allowed</returns>
    public bool? UserRestrictedByLanguagePermissions(IAppReader appReader)
    {
        var l = Log.Fn<bool?>($"{appReader.Name}({appReader.AppId})");
        // Note: it's important that all cases where we don't detect a forbidden
        // we return null, and DON'T access _ctx.UserMayEdit, as it will recurse to here again
        if (!featuresLazy.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            return l.ReturnNull("feat disabled");

        // Check if we have any language rules
        var languages = LanguagesWithPermissions(appReader);
        if (languages == null || languages.Count == 0) return l.ReturnNull("no config");

        // Check rules on current language
        var currentCode = ctx.Site.CurrentCultureCode;
        var currentLang = languages.FirstOrDefault(lp => lp.Code.Equals(currentCode, InvariantCultureIgnoreCase));
        return l.Return(currentLang?.IsAllowed, $"permission: {currentLang?.IsAllowed}");
    }

    /// <summary>
    /// Figure out what languages exist and if the current user has permissions to edit them.
    /// </summary>
    /// <param name="appReaderOrNull"></param>
    /// <returns></returns>
    public List<AppUserLanguageState> LanguagesWithPermissions(IAppReader appReaderOrNull)
    {
        var l = Log.Fn<List<AppUserLanguageState>>();
        // to solves the issue with globals settings languages that can not be saved if 
        // app languages are different from languages in global app and because global
        // settings are in primary appid=1, zoneId=1 without portal site we just return empty list for it
        // in other cases we get the languages from the app state or from context (http headers)
        var zoneMapper = zoneMapperLazy.Value;
        var site = appReaderOrNull != null
            ? zoneMapper.SiteOfZone(appReaderOrNull.ZoneId)
            : ctx.Site;
        if (site == null)
            return l.Return([], "null site");

        var languages = zoneMapper.CulturesWithState(site);

        // Check if ML-Permissions-Feature is enabled, otherwise don't check detailed permissions
        var mlFeatureEnabled = featuresLazy.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage);
        var allowAllLanguages = !mlFeatureEnabled || ctx.User.IsSystemAdmin;

        if (allowAllLanguages || appReaderOrNull == null)
        {
            var noAppResult = languages
                .Select<ISiteLanguageState, AppUserLanguageState>(lng => new(lng, true, -1))
                .ToList();
            return l.Return(noAppResult, $"no-app {noAppResult.Count}");
        }

        var appStateSafe = appReaderOrNull;

        var set = GetLanguagePermissions(appStateSafe, languages);
        l.A($"Found {set.Count} sets");
        var hasPermissions = set.Any(s => s.Permissions.Any());

        // Find primary app, or stop if we're already there
        if (!hasPermissions && appStateSafe.NameId != Constants.PrimaryAppGuid)
        {
            l.A("No permissions, and not primary app - will try that");
            var primaryAppReader = appReadersLazy.Value.GetPrimaryReader(appStateSafe.ZoneId, Log);
            set = GetLanguagePermissions(primaryAppReader, languages);
            hasPermissions = set.Any(s => s.Permissions.Any());
        }

        var defaultAllowed = ctx.User.IsSystemAdmin || !hasPermissions;
        l.A($"HasPermissions: {hasPermissions}, Initial Allowed: {defaultAllowed}");

        var newSet = set.Select(s =>
        {
            var permissionEntities = s.Permissions.ToList();
            var ok = defaultAllowed;
            if (!ok)
            {
                var pChecker = checkGenerator.New();
                var permissions = permissionEntities.Select(p => new Permission(p));
                pChecker.ForCustom(ctx, appStateSafe, permissions);
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
        return l.Return(result, $"ok {result.Count}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mdSource">The AppState which could hold permissions - or null if the app isn't there yet (like adding new module)</param>
    /// <param name="languages"></param>
    /// <returns></returns>
    private static List<LanguagePermission> GetLanguagePermissions(IMetadataSource mdSource, List<ISiteLanguageState> languages)
    {
        var set = languages
            .Select(l => new LanguagePermission
            {
                Permissions = mdSource?.GetMetadata(TargetTypes.Dimension, l.Code?.ToLowerInvariant(), Permission.TypeName)
                              ?? [],
                Language = l
            })
            .ToList();
        return set;
    }

    /// <summary>
    /// Temporary information holding a language and permission entities for it.
    /// </summary>
    private class LanguagePermission
    {
        public ISiteLanguageState Language;
        public IEnumerable<IEntity> Permissions;
    }
}