/*
 * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
 *
 * This file and the code IS COPYRIGHTED.
 * 1. You may not change it.
 * 2. You may not copy the code to reuse in another way.
 *
 * Copying this or creating a similar service, 
 * especially when used to circumvent licensing features in EAV and 2sxc
 * is a copyright infringement.
 *
 * Please remember that 2sic has sponsored more than 10 years of work,
 * and paid more than 1 Million USD in wages for its development.
 * So asking for support to finance advanced features is not asking for much. 
 *
 */

using System.Collections.Immutable;
using ToSic.Sys.Capabilities.FeatureSet;
using ToSic.Sys.Utils;

namespace ToSic.Sys.Capabilities.Licenses;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LicenseService : ILicenseService
{
    #region Public APIs

    /// <inheritdoc />
    public IList<FeatureSetState> All => AllStaticCache;

    /// <inheritdoc />
    /// <remarks>
    /// We use the real static LicenseDefinition as an index, because this ensures that people can't inject other license objects to bypass security.
    /// </remarks>
    public IImmutableDictionary<Guid, FeatureSetState> Enabled
        => EnabledStaticCache;

    /// <inheritdoc />
    public bool IsEnabled(FeatureSet.FeatureSet license)
        => EnabledStaticCache.ContainsKey(license.Guid);

    public FeatureSetState? State(FeatureSet.FeatureSet license)
        => EnabledStaticCache.GetValueOrDefault(license.Guid);

    #endregion

    #region Internal stuff, caching, static

    private static IList<FeatureSetState> AllStaticCache { get; set; } = [];


    private static IImmutableDictionary<Guid, FeatureSetState> EnabledStaticCache { get; set; }
        = ImmutableDictionary<Guid, FeatureSetState>.Empty;

    public static long CacheTimestamp;


    public static void Update(IList<FeatureSetState> licenses)
    {
        AllStaticCache = licenses;
        EnabledStaticCache = licenses
            .Where(l => l.IsEnabled)
            .DistinctByLongestExpiration()
            .ToImmutableDictionary(l => l.Aspect.Guid, l => l); ;
        CacheTimestamp = DateTime.Now.Ticks;
        
        // On every license update, store if we have any valid licenses
        // This is for faster checks on each module load, to verify we don't need to show
        // upgrade warnings
        _allRegisteredLicensesAreInvalid = !AreAnyRegisteredLicensesStillValid(licenses);
    }

    /// <inheritdoc/>
    public bool AllRegisteredLicensesAreInvalid => _allRegisteredLicensesAreInvalid;
        
    private static bool _allRegisteredLicensesAreInvalid = true;

    private static bool AreAnyRegisteredLicensesStillValid(IList<FeatureSetState> licenses)
    {
        // We must only check the registered ones (which are not auto-enabled)
        var registeredLicenses = licenses
            .Where(l => !l.Aspect.AutoEnable)
            .ToListOpt();
        // If there are no registered licenses, then everything is fine
        // Otherwise check that there is at least 1 valid license
        return registeredLicenses.SafeNone() || registeredLicenses.Any(license => license.Valid);
    }
    #endregion
}