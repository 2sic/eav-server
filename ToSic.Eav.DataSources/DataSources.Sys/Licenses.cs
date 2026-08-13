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
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Scopes DS
    /// </summary>
    [PrivateApi]
    public Licenses(Dependencies services, ILicenseService licenseService) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Lics", connect: [licenseService])
    {
        ProvideOutRaw(
            () => licenseService.All
                .OrderBy(license => license.Aspect?.Priority ?? 0)
                .Select(license => new FeatureSetStateRaw(license)),
            options: () => new() { TypeName = "License" }
        );
    }
}