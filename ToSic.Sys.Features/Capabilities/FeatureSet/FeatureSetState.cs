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

using ToSic.Sys.Capabilities.Aspects;

namespace ToSic.Sys.Capabilities.FeatureSet;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record FeatureSetState(FeatureSet Aspect)
    : AspectState<FeatureSet>(Aspect, true)
{
    /* def true as otherwise we wouldn't have the config */

    public required string Title { get; init; }
    public string? LicenseKey { get; init; }

    public Guid EntityGuid { get; init; }

    // public FeatureSet Aspect { get; internal set; }

    public override bool IsEnabled => EnabledInConfiguration && Valid;

    /// <summary>
    /// The state as toggled in the settings - ATM always true, as we don't read the settings
    /// </summary>
    public bool EnabledInConfiguration => true;

    public bool Valid => ExpirationIsValid && SignatureIsValid && FingerprintIsValid && VersionIsValid;

    public DateTime Expiration { get; init; }

    public bool ExpirationIsValid { get; init; }

    public bool SignatureIsValid { get; init; }

    public bool FingerprintIsValid { get; init; }

    public bool VersionIsValid { get; init; }
        
    public string? Owner { get; init; }

}