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

using ToSic.Eav.Internal.Catalogs;
using ToSic.Eav.SysData;
using static ToSic.Eav.Internal.Licenses.BuiltInLicenses;

namespace ToSic.Eav.Internal.Licenses;

public class LicenseCatalog: GlobalCatalogBase<FeatureSet>
{
    public LicenseCatalog(ILogStore logStore): base(logStore, $"{EavLogs.Eav}.LicCat", new())
    {
        Register(
            CoreFree,
            CorePlus,
            CoreBeta,
            PatronBasic,
            PatronAdvancedCms,
            PatronPerfectionist,
            PatronSentinel,
            PatronSuperAdmin,
            PatronInfrastructure,
            WebFarmCache,
            EnterpriseCms,

            BuiltInLicenses.System,
            Extension

#if DEBUG
            // disable in production
            ,
            CoreTesting
#endif
        );
    }

    public override FeatureSet TryGet(string name) =>
        name == null
            ? null
            : base.TryGet(name)
              ?? Dictionary.Values.FirstOrDefault(lic => name.Equals(lic.Guid.ToString()));
}