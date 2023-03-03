using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data.Factory;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

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
        GlobalName = "402fa226-5584-46d1-a763-e63ba0774c31",
        Audience = Audience.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Licenses : DataSource
    {
        private readonly IDataBuilder _builder;

        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Licenses(MyServices services, ILicenseService licenseService, IDataBuilder dataBuilder) : base(services, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _licenseService = licenseService,
                _builder = dataBuilder.Configure(typeName: "License")
            );
            Provide(GetList);
        }
        private readonly ILicenseService _licenseService;


        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
            // Don't parse configuration as there is nothing to configure
            // Configuration.Parse();

            var list = _builder.Build(_licenseService.All.OrderBy(l => l.License?.Priority ?? 0));
            
            return (list, $"{list.Count}");
        });
    }
}