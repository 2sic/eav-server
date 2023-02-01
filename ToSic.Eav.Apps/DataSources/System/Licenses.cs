using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Data;
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
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Licenses : DataSource
    {
        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Licenses(Dependencies dependencies, ILicenseService licenseService) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _licenseService = licenseService
            );
            Provide(GetList);
        }
        private readonly ILicenseService _licenseService;


        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            // Don't parse configuration as there is nothing to configure
            // Configuration.Parse();

            var licenseBuilder = new DataBuilderQuickWIP(DataBuilder, appId: Constants.PresetAppId, typeName: "License", titleField: Data.Attributes.TitleNiceName);
            var list = _licenseService.All
                .OrderBy(lic => lic.License?.Priority ?? 0)
                .Select(lic => licenseBuilder.Create(lic))
                .ToImmutableArray();
            
            return (list, $"{list.Length}");
        });
    }
}