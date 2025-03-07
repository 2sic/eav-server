using ToSic.Eav.TestData;

namespace ToSic.Eav.DataSourceTests;

[TestClass]
public class EntityTypeFilterTest: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void EntityTypeFilter_FindAllIfAllApply()
    {
        var vf = CreateEntityTypeFilterForTesting(1000);
        vf.TypeName = "Person";
        AreEqual(1000, vf.ListTac().Count(), "Should find all");
    }

    [TestMethod]
    public void EntityTypeFilter_FindNoneIfNoneApply()
    {
        var vf = CreateEntityTypeFilterForTesting(1000);
        vf.TypeName = "Category";
        AreEqual(0, vf.ListTac().Count(), "Should find all");
    }




    public EntityTypeFilter CreateEntityTypeFilterForTesting(int testItemsInRootSource)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001);
        var filtered = DsSvc.CreateDataSource<EntityTypeFilter>(ds);// DataSourceFactory.GetDataSource<EntityTypeFilter>(ds);
        return filtered;
    }
}