using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.SysData;
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
    Audience = Audience.Advanced,
    DynamicOut = false
)]
// ReSharper disable once UnusedMember.Global
public sealed class Licenses : CustomDataSource
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Scopes DS
    /// </summary>
    [PrivateApi]
    public Licenses(MyServices services, ILicenseService licenseService) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Lics")
    {
        ConnectLogs([licenseService]);
        ProvideOutRaw(
            () => licenseService.All
                .OrderBy(l => l.Aspect?.Priority ?? 0)
                .Select(l => l.ToRawEntity2()),
            options: () => new() { TypeName = "License" }
        );
    }
}