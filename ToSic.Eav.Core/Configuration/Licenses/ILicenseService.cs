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
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ToSic.Eav.Configuration.Licenses
{
    public interface ILicenseService
    {
        /// <summary>
        /// All licenses
        /// </summary>
        List<LicenseState> All { get; }

        /// <summary>
        /// Enabled licenses, in a dictionary to retrieve with the LicenseDefinition object
        /// </summary>
        IImmutableDictionary<LicenseDefinition, LicenseState> Enabled { get; }

        /// <summary>
        /// Check if a license is enabled - using the real primary LicenseDefinition object as the key.
        /// </summary>
        /// <returns></returns>
        bool IsEnabled(LicenseDefinition license);

        /// <summary>
        /// Check if any license is valid.
        /// </summary>
        bool HaveValidLicense { get;  }
    }
}