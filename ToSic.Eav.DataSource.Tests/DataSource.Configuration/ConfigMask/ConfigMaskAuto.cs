using ToSic.Eav.DataSource.Internal.Configuration;

namespace ToSic.Eav.DataSource.Configuration;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class ConfigMaskAuto(DataSourcesTstBuilder dsSvc, ConfigurationDataLoader findConfigs)
{
    [Fact]
    public void EnsureNoCacheAtFirstAndCacheLater()
    {
        // Make sure it's reset
        ConfigurationDataLoader.Cache = new();

        // should be false at first
        False(ConfigurationDataLoader.Cache.ContainsKey(typeof(Shuffle)));
        var masks = findConfigs.GetTokensTac(typeof(Shuffle));
        True(ConfigurationDataLoader.Cache.ContainsKey(typeof(Shuffle)));
    }

    [Fact]
    public void GetMasksForShuffle()
    {
        var masks = findConfigs.GetTokensTac(typeof(Shuffle));
        Single(masks);
        Equal($"[{DataSourceConstants.MyConfigurationSourceName}:Take||0]", masks.First().Token);
    }

    [Fact]
    public void GetMasksForInheritedDataSource()
    {
        var masks = findConfigs.GetTokensTac(typeof(Children));
        Equal(3, masks.Count);
        Equal($"[{DataSourceConstants.MyConfigurationSourceName}:{nameof(Children.FieldName)}]", masks.Skip(1).First().Token);
    }

    [Fact]
    public void GetMasksWithOtherFieldName()
    {
        var masks = findConfigs.GetTokensTac(typeof(RelationshipFilter));
        Equal(6, masks.Count);
        Equal($"[{DataSourceConstants.MyConfigurationSourceName}:Direction||{RelationshipFilter.DefaultDirection}]", masks.First(m => m.Key == nameof(RelationshipFilter.ChildOrParent)).Token);
    }

    [Fact]
    public void GetMasksWithEnumDefault()
    {
        var masks = findConfigs.GetTokensTac(typeof(RelationshipFilter));
        Equal(6, masks.Count);
        Equal($"[{DataSourceConstants.MyConfigurationSourceName}:Comparison||contains]", masks.First(m => m.Key == nameof(RelationshipFilter.CompareMode)).Token);
    }

    [Fact]
    public void ShuffleShouldHave1Mask()
    {
        var shuffle = dsSvc.CreateDataSource<Shuffle>();
        Single(shuffle.Configuration.Values);
    }

}