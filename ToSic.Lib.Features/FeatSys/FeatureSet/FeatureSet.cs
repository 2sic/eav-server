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

namespace ToSic.Eav.SysData;

/// <summary>
/// Defines a license - name, guid etc.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record FeatureSet :  Aspect
{
    public FeatureSet() { }

    public FeatureSet(
        string NameId,
        int Priority,
        string Name,
        Guid Guid,
        string Description,
        bool FeatureLicense = false)
        : base(NameId ?? Guid.ToString(), Guid, Name, Description ?? "")
    {
        this.Priority = Priority;
        this.FeatureLicense = FeatureLicense;
    }

    public const string ConditionIsLicense = "license";

    public int Priority { get; init; }

    public bool AutoEnable { get; init; } = false;

    public FeatureSet[] AlsoInheritEnabledFrom { get; init; }= [];

    public bool FeatureLicense { get; init; }

    public Requirement Requirement => field ??= new(ConditionIsLicense, Guid.ToString());
}