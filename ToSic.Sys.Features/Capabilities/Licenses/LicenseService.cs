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
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Licenses;

public class LicenseService : ILicenseService
{
    #region Public APIs

    /// <inheritdoc />
    public List<FeatureSetState> All => AllCache;

    /// <inheritdoc />
    /// <remarks>
    /// We use the real static LicenseDefinition as an index, because this ensures that people can't inject other license objects to bypass security.
    /// </remarks>
    public IImmutableDictionary<Guid, FeatureSetState> Enabled
        => EnabledCache;

    /// <inheritdoc />
    public bool IsEnabled(FeatureSet license)
        => EnabledCache.ContainsKey(license.Guid);

    public FeatureSetState? State(FeatureSet license)
        => EnabledCache.TryGetValue(license.Guid, out var result) ? result : null;

    #endregion

    #region Internal stuff, caching, static

    private static List<FeatureSetState> AllCache { get; set; } = [];


    private static IImmutableDictionary<Guid, FeatureSetState> EnabledCache { get; set; } =
        new Dictionary<Guid, FeatureSetState>().ToImmutableDictionary();

    public static long CacheTimestamp;


    public static void Update(List<FeatureSetState> licenses)
    {
        AllCache = licenses;
        EnabledCache = licenses
            .Where(l => l.IsEnabled)
            .OrderByDescending(l => l.Expiration) // same feature license with longer expiration have priority
            // must do Distinct = GroupBy+First to ensure we don't have duplicate keys
            .GroupBy(l => l.Aspect)
            .Select(g => g.First())
            .ToImmutableDictionary(l => l.Aspect.Guid, l => l); ;
        CacheTimestamp = DateTime.Now.Ticks;
        AllLicensesAreInvalid = AreAllLicensesInvalid();
    }

    public bool HaveValidLicense => !AllLicensesAreInvalid;
        
    internal static bool AllLicensesAreInvalid = false;

    internal static bool AreAllLicensesInvalid()
    {
        // if we do not have license for validation, than it can not be invalid
        if (AllCache.All(l => l.Aspect.AutoEnable)) return false;
            
        // any valid license?
        foreach (var license in AllCache.Where(l => !l.Aspect.AutoEnable))
            if (license.Valid)
                return false;
        return true;
    }
    #endregion
}