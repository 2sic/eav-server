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

using ToSic.Eav.Data;

namespace ToSic.Eav.Configuration.Licenses
{
    internal class LicenseConstants
    {
        public static string TypeNameId = "736b3da6-7d3f-4147-ad09-5d311446f575";
        public static string TypeName = "SystemLicense";
        public static string FieldTitle = Attributes.TitleNiceName;
        public static string FieldKey = "LicenseKey";
        public static string FieldLicenses = "Licenses";
        public static string FieldFingerprint = "Fingerprints";
        public static string FieldSignature = "Signature";
        public static string FieldRestrictions = "Restrictions";
    }
}
