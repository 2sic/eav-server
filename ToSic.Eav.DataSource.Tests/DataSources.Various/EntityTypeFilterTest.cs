using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSources.Various;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class EntityTypeFilterTest(DataSourcesTstBuilder DsSvc, Generator<DataTablePerson> personTableGenerator)
{
    [Fact]
    public void EntityTypeFilter_FindAllIfAllApply()
    {
        var vf = CreateEntityTypeFilterForTesting(1000);
        vf.TypeName = "Person";
        Equal(1000, vf.ListTac().Count());//, "Should find all");
    }

    [Fact]
    public void EntityTypeFilter_FindNoneIfNoneApply()
    {
        var vf = CreateEntityTypeFilterForTesting(1000);
        vf.TypeName = "Category";
        Equal(0, vf.ListTac().Count());//, "Should find all");
    }




    public EntityTypeFilter CreateEntityTypeFilterForTesting(int testItemsInRootSource)
    {
        var ds = personTableGenerator.New().Generate(testItemsInRootSource, 1001);
        var filtered = DsSvc.CreateDataSource<EntityTypeFilter>(ds);// DataSourceFactory.GetDataSource<EntityTypeFilter>(ds);
        return filtered;
    }
}