using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration;
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
        NiceName = "Features",
        UiHint = "List all features",
        Icon = Icons.TableChart,
        Type = DataSourceType.System,
        GlobalName = "398d0b9f-044f-48f7-83ef-307872f7ed93",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Features : DataSource
    {
        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Features(Dependencies dependencies, IFeaturesInternal featuresService) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _featuresService = featuresService
            );
            Provide(GetList);
        }
        private readonly IFeaturesInternal _featuresService;


        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            // Don't parse configuration as there is nothing to configure
            // Configuration.Parse();

            var featureBuilder = new DataBuilderQuickWIP(DataBuilder, appId: Constants.PresetAppId, typeName: "Feature", titleField: Data.Attributes.TitleNiceName);
            var list = _featuresService.All
                .OrderBy(f => f.NameId)
                .Select(f => featureBuilder.Create(f))
                .ToImmutableArray();
            
            return (list, $"{list.Length}");
        });
    }
}