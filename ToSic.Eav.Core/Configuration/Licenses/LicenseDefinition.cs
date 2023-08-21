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
using ToSic.Lib.Data;

namespace ToSic.Eav.Configuration.Licenses
{
    /// <summary>
    /// Defines a license - name, guid etc.
    /// </summary>
    public class LicenseDefinition: IHasIdentityNameId
    {
        public const string ConditionIsLicense = "license";

        public LicenseDefinition(int priority, string name, Guid guid, string description, bool featureLicense = false)
        {
            Priority = priority;
            Name = name;
            Guid = guid;
            Description = description ?? "";
            Condition = new Condition(ConditionIsLicense, guid.ToString());
            FeatureLicense = featureLicense;
        }

        public int Priority { get; }

        public string Name { get; }

        public string NameId => Guid.ToString();

        public Guid Guid { get; }
        public bool AutoEnable { get; set; } = false;
        public string Description { get; }
        public LicenseDefinition[] AlsoInheritEnabledFrom = Array.Empty<LicenseDefinition>();

        public bool FeatureLicense { get; }

        public Condition Condition { get; }
    }
}
