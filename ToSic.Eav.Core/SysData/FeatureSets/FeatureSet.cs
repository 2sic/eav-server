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

using System;

namespace ToSic.Eav.SysData;

/// <summary>
/// Defines a license - name, guid etc.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FeatureSet(
    string nameId,
    int priority,
    string name,
    Guid guid,
    string description,
    bool featureLicense = false)
    : Aspect(nameId ?? guid.ToString(), guid, name, description ?? "")
{
    public const string ConditionIsLicense = "license";

    public int Priority { get; } = priority;

    public bool AutoEnable { get; set; } = false;

    public FeatureSet[] AlsoInheritEnabledFrom { get; set; }= Array.Empty<FeatureSet>();

    public bool FeatureLicense { get; } = featureLicense;

    public Requirement Requirement { get; } = new(ConditionIsLicense, guid.ToString());
}