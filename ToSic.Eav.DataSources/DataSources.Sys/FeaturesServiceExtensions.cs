using ToSic.Eav.Data.Build;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// Mini Helper for DataSources which will probably be accessed through SysData.
/// This is because often the data shown may need to also provide some information about the feature activation.
/// </summary>
/// <param name="featureSvc"></param>
/// <param name="dataFactory"></param>
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
        var df = dataFactory.SpawnNew(options: new()
        {
            AutoId = false,
            TypeName = FeaturesToRawEntity.FeatureStateTypeName,
        });

        var featState = featureSvc.Get(feature.NameId)!.ToRawEntity();

        var converted = df.Create(featState);
        
        return [converted];
    }
}
