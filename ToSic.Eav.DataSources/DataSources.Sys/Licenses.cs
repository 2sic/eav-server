using ToSic.Eav.DataSource.Sys;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that list all features.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "Licenses",
    UiHint = "List all licenses",
    Icon = DataSourceIcons.TableChart,
    Type = DataSourceType.System,
    NameId = "402fa226-5584-46d1-a763-e63ba0774c31",
    NameIds = ["System.Licenses"],
    Audience = Audience.Advanced
)]
// ReSharper disable once UnusedMember.Global
public sealed class Licenses : CustomDataSource
{
    [PrivateApi]
    public Licenses(Dependencies services, ILicenseService licenseService)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Lics", connect: [licenseService])
    {
        
        ProvideOutRaw(() => licenseService.All
            .DistinctByLongestExpiration()
            .Select(l => new FeatureSetStateRaw(l))
            .OrderBy(l => l.Priority)
            .ToListOpt());

        // Note: old code till 2026-08-26 2dm #ToRemoveQ4
        // This resulted in certain licenses being listed multiple times
        // From my understanding it's because the license service has multiple entries for the same license, but with different expiration dates.
        // This caused problems in the UI for managing licenses, and I think it was never expected behavior
        // The only other place where this DataSource is currently used seems to be in a dropdown of data,
        // and I believe it also doesn't need the duplicates.
        //
        //    return licenseService.All
        //        //.OrderBy(license => license.Aspect.Priority)
        //        .Select(license => new FeatureSetStateRaw(license))
        //        .OrderBy(l => l.Priority)
        //        .ToListOpt();

    }
}