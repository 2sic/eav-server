using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that list all features.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "Feature States",
    UiHint = "List all features states",
    Type = DataSourceType.System,
    NameId = "9c92f05c-ac1e-419d-8d55-3be46660aaa1",
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System
)]
// ReSharper disable once UnusedMember.Global
public sealed class FeatureStates : CustomDataSource
{
    /// <summary>
    /// Optional filter to only return specific features by their NameId, comma-separated. E.g. "Feature1,Feature2"
    /// </summary>
    /// <remarks>
    /// If blank or not set, will return all feature states.
    /// 
    /// Added in v21.02
    /// </remarks>
    [Configuration(Fallback = "")]
    public string FeatureId => Configuration.GetThis<string>("");

    [PrivateApi]
    public FeatureStates(Dependencies services, ISysFeaturesService featuresService) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.FState", connect: [featuresService])
    {
        ProvideOutRaw(
            () => GetList(featuresService),
            options: () => new() { TypeName = FeaturesToRawEntity.FeatureStateTypeName }
        );
    }

    private IEnumerable<IRawEntity> GetList(ISysFeaturesService featuresService)
    {
        var filterIds = FeatureId.CsvToArrayWithoutEmpty();

        if (!filterIds.Any())
            return [];

        var mainList = featuresService.All
            .Where(f => filterIds.Contains(f.NameId, StringComparer.InvariantCultureIgnoreCase));

        var result = mainList
            .OrderBy(f => f.NameId)
            .Select(f => f.ToRawEntity(detailed: true));

        return result;
    }
}