﻿using System.Diagnostics.CodeAnalysis;
using ToSic.Sys.Caching;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Utils;
using static System.StringComparer;


namespace ToSic.Sys.Capabilities.Features;

/// <summary>
/// WARNING: this is used as a singleton / static
/// not quite sure anymore why, either the name is hard-coded in some apps or we felt like it's a performance improvement
/// </summary>
/// <remarks>
/// WARNING: singleton - don't use any complex services/dependencies here
/// REASON is probably cache connection, since it should notify the system to release caches as settings change?
///
/// 2024-05-31 changing to non-singleton, must monitor if all is ok...
/// 2025-03-11 updated the `CacheTimestamp` property to use a backing field `_staticCacheTimestamp`, 
///            so WebFarmCache is working when feature is enabled https://github.com/2sic/2sxc/issues/3597
/// </remarks>
[PrivateApi("hide implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LibSysFeaturesService(FeaturesCatalog featuresCatalog) : ISysFeaturesService, ILibFeaturesService
{
    public IEnumerable<FeatureState> All => _allStaticCache ??= Merge(Stored, featuresCatalog.List, _staticSysFeatures);
    private static IList<FeatureState>? _allStaticCache;

    /// <summary>
    /// List of all enabled features with their guids and nameIds
    /// </summary>
    private HashSet<string> EnabledFeatures => _enabledFeatures ??= new(All
            .Where(f => f.IsEnabled)
            .OrderBy(f => f.NameId) // order them so it's easier to debug
            .SelectMany(f => new[] { f.NameId, f.Aspect.Guid.ToString() })
            .Distinct(InvariantCultureIgnoreCase),
        InvariantCultureIgnoreCase);
    private HashSet<string>? _enabledFeatures; // Do not use GetOnce, because of "Error Unable to marshal host object to interpreter space"

    public IEnumerable<FeatureState> UiFeaturesForEditors
        => All.Where(f => f.IsEnabled && f.IsForEditUi);

    public bool IsEnabled(Guid guid)
        => All.Any(f => f.Aspect.Guid == guid && f.IsEnabled);
        
    public bool IsEnabled(IEnumerable<Guid> guids)
        => guids.All(IsEnabled);

    public bool IsEnabled(params string[]? nameIds)
        => nameIds == null || nameIds.Length == 0 || nameIds.All(name => EnabledFeatures.Contains(name?.Trim() ?? ""));

    public FeatureState? Get(string nameId)
        => All.FirstOrDefault(f => f.Aspect.Name == nameId || f.NameId == nameId);

    public bool IsEnabled(params Feature[] features) 
        => IsEnabled(features?.Select(f => f.NameId).ToArray());

    public bool Valid => ValidInternal;
    public static bool ValidInternal; // ATM always false; is used by a static class - not sure why this even exists as I don't think it's set anywhere
        
    public bool IsEnabled(IEnumerable<Guid> features, string message, [NotNullWhen(false)] out FeaturesDisabledException? exception)
    {
        // ReSharper disable PossibleMultipleEnumeration
        var enabled = IsEnabled(features);
        exception = enabled
            ? null
            : new FeaturesDisabledException(message + " - " + MsgMissingSome(features.ToArray()));
        // ReSharper restore PossibleMultipleEnumeration
        return enabled;
    }

    [PrivateApi]
    public string MsgMissingSome(params Guid[] ids)
    {
        var missing = ids
            .Where(i => !IsEnabled(i))
            .Select(id => new
            {
                Id = id,
                All.FirstOrDefault(f => f.Aspect.Guid == id)?.NameId
            });

        var messages = missing.Select(f => $"'{f.NameId}'");

        return $"Features {string.Join(", ", messages)} not enabled - see also https://go.2sxc.org/features";
    }


    #region Static Caches

    [PrivateApi]
    public FeatureStatesPersisted? Stored => _staticStored;

    private static FeatureStatesPersisted? _staticStored;
    private static IList<FeatureState>? _staticSysFeatures;

    public bool UpdateFeatureList(FeatureStatesPersisted newList, IList<FeatureState> sysFeatures)
    {
        _staticStored = newList;
        _staticSysFeatures = sysFeatures;
        _allStaticCache = null;
        _enabledFeatures = null;
        _staticCacheTimestamp = DateTime.Now.Ticks;

        // Notify the cache that the features have changed
        MemoryCacheService.Notify(this);

        return true;
    }


    private static IList<FeatureState> Merge(FeatureStatesPersisted? config, IReadOnlyCollection<Feature> featuresCat, IList<FeatureState>? sysFeatureStates)
    {
        var licService = new LicenseService();

        var allFeats = featuresCat
            .Select(featDef =>
            {
                var enabled = false;
                var licenseEnabled = false;
                var msgShort = "default";
                var message = " by default";
                var expiry = DateTime.MinValue;

                // Check if the required license is active
                var enabledRule = featDef.LicenseRulesList.FirstOrDefault(lr => licService.IsEnabled(lr.FeatureSet));
                if (enabledRule != null)
                {
                    licService.Enabled.TryGetValue(enabledRule.FeatureSet.Guid, out var licState);
                    expiry = licState?.Expiration ?? LicenseConstants.UnlimitedExpiry;
                    enabled = enabledRule.EnableFeatureByDefault;
                    licenseEnabled = true; // The license is active, so it's allowed to enable this
                    msgShort = "license ok";
                    message = $" by default with license {enabledRule.FeatureSet.Name}";
                }

                // Check if the configuration would enable this feature
                var inConfig = config?.Features.FirstOrDefault(cf => cf.Id == featDef.Guid);
                if (inConfig != null)
                {
                    enabled = licenseEnabled && inConfig.EnabledRespectingExpiry();
                    if (expiry == DateTime.MinValue)
                        expiry = inConfig.Expires; // set expiry by configuration (when is not set by license)
                    msgShort = licenseEnabled ? "configuration" : "unlicensed";
                    message = licenseEnabled ? " by configuration" : " - requires license";
                }

                return new FeatureState(
                    featDef,
                    expiry,
                    enabled,
                    msgShort,
                    (enabled ? "Enabled" : "Disabled") + message,
                    licenseEnabled,
                    EnabledByDefault: enabledRule?.EnableFeatureByDefault ?? false,
                    EnabledInConfiguration: inConfig?.Enabled,
                    Configuration: inConfig?.Configuration
                );
            })
            .ToListOpt();

        // Find additional, un matching features which are not known in the catalog
        var missingFeatures = config?.Features
            .Where(f => featuresCat.All(fd => fd.Guid != f.Id))
            .Select(f => new FeatureState(
                    Feature.UnknownFeature(f.Id),
                    f.Expires,
                    f.EnabledRespectingExpiry(),
                    "configuration",
                    "Configured manually",
                    AllowedByLicense: false,
                    EnabledByDefault: false,
                    EnabledInConfiguration: f.Enabled != null,
                    Configuration: f.Configuration
                )
            );

        var final = (missingFeatures == null
                ? allFeats
                : allFeats.Union(missingFeatures))
            .ToListOpt();

        if (sysFeatureStates.SafeAny())
            final = final
                .Union(sysFeatureStates)
                .ToListOpt();
        return final;
    }

    [PrivateApi]
    public long CacheTimestamp => _staticCacheTimestamp; // fix https://github.com/2sic/2sxc/issues/3597
    private static long _staticCacheTimestamp; // need static to preserve value across transient instances

    public bool CacheChanged(long dependentTimeStamp)
        => CacheTimestamp != dependentTimeStamp;

    #endregion

    #region CacheDependency

    public bool CacheIsNotifyOnly => true;

    string ICanBeCacheDependency.CacheDependencyId => typeof(ISysFeaturesService).FullName!;

    bool ILibFeaturesService.IsEnabled(string nameIds) => IsEnabled([nameIds]);

    #endregion

}