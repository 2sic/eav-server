using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSources.Various;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class EntityIdFilterTest(DataSourcesTstBuilder DsSvc, Generator<DataTablePerson> personTableGenerator)
{
    [Fact]
    public void EntityIdFilter_SingleItem()
    {
        const string itemToFilter = "1023";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListTac();

        Equal(itemToFilter, ll.First().EntityId.ToString());
    }


    [Fact]
    public void EntityIdFilter_NoItems()
    {
        const string itemToFilter = "";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListTac();

        Empty(ll);//, "Should return 0 items");
    }

    [Fact]
    public void EntityIdFilter_MultipleItems()
    {
        const string itemToFilter = "1011,1023,1050,1003";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListTac();
        Equal("1011", ll.First().EntityId.ToString());//, "Test Light that sorting IS affeted");
        Equal(4, ll.Count());//, "Count after filtering");
    }

    [Fact]
    public void EntityIdFilter_FilterWithSpaces()
    {
        const string itemToFilter = "1011, 1023 ,1050   ,1003";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListTac();
        Equal("1011", ll.First().EntityId.ToString());//, "Test Light that sorting IS affeted");
        Equal(4, ll.Count());//, "Count after filtering");
    }

    [Fact]
    public void EntityIdFilter_CacheKey1()
    {
        // Simple scenario, 1 filter, no special spaces etc.
        const string itemToFilter = "1023";
        var ds = CreateFilterForTesting(100, itemToFilter);

        Equal("EntityIdFilter:NoGuid&EntityIds=1023", ds.CachePartialKey);
        Equal("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1023", ds.CacheFullKey);
        var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
        True(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");

    }
    [Fact]
    public void EntityIdFilter_CacheKeyMulti()
    {
        // Multi-value scenario, no special spaces etc.
        var itemToFilter = "1011,1023,1050,1003";
        var ds = CreateFilterForTesting(100, itemToFilter);

        Equal("EntityIdFilter:NoGuid&EntityIds=1011,1023,1050,1003", ds.CachePartialKey);
        Equal("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1011,1023,1050,1003", ds.CacheFullKey);

        // Multi-value scenario, special spaces and trailing comma etc.
        itemToFilter = "1011, 1023  ,   1050,    1003,";
        var partialKey = "EntityIdFilter:NoGuid&EntityIds=1011,1023,1050,1003,";
        ds = CreateFilterForTesting(100, itemToFilter);
        Equal(partialKey, ds.CachePartialKey);
        Equal("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>" + partialKey, ds.CacheFullKey);

    }

    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
    {
        var ds = personTableGenerator.New().Generate(testItemsInRootSource, 1001);
        var filtered = DsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachTac(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}