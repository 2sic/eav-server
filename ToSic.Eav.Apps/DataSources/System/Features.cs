using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data.Build;
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
        NiceName = "Features",
        UiHint = "List all features",
        Icon = Icons.TableChart,
        Type = DataSourceType.System,
        GlobalName = "398d0b9f-044f-48f7-83ef-307872f7ed93",
        Audience = Audience.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Features : DataSource
    {

        #region Configuration-properties (no config)

        #endregion


        private readonly IDataFactory _factory;

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Features(MyServices services, IFeaturesInternal featuresService, IDataFactory dataFactory) : base(services, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _featuresService = featuresService,
                _factory = dataFactory.New(settings: new DataFactorySettings(typeName: "Feature"))
            );
            Provide(GetList);
        }
        private readonly IFeaturesInternal _featuresService;


        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            // Don't parse configuration as there is nothing to configure
            // Configuration.Parse();

            var list = _factory.Create(_featuresService.All.OrderBy(f => f.NameId));

            return (list, $"{list.Count}");
        });
    }
}