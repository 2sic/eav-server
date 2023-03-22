using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that list all features.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Licenses",
        UiHint = "List all licenses",
        Icon = Icons.TableChart,
        Type = DataSourceType.System,
        NameId = "402fa226-5584-46d1-a763-e63ba0774c31",
        Audience = Audience.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Licenses : CustomDataSourceLight
    {
        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Licenses(MyServices services, ILicenseService licenseService) : base(services, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(licenseService);
            ProvideOutRaw(() => licenseService.All.OrderBy(l => l.License?.Priority ?? 0), options: () => new DataFactoryOptions(typeName: "License"));
        }
    }
}