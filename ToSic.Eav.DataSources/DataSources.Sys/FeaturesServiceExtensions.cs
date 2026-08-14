using ToSic.Eav.Data.Build;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// Mini Helper for DataSources which will probably be accessed through SysData.
/// This is because often the data shown may need to also provide some information about the feature activation.
/// </summary>
/// <param name="featureSvc"></param>
/// <param name="dataFactory"></param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class FeaturesForDataSources(ISysFeaturesService featureSvc, IDataFactory dataFactory)
{
    /// <summary>
    /// Recommended Stream name
    /// </summary>
    public const string StreamName = "Feature";

    /// <summary>
    /// Public feature service, as the DS will usually need this as well.
    /// </summary>
    public ISysFeaturesService Features => featureSvc;

    public IEnumerable<IEntity> GetDataForFeature(Feature feature)
    {
        var featureState = new FeatureStateMinimalRaw(featureSvc.Get(feature.NameId)!);
        return [dataFactory.Create(featureState)];
    }
}
