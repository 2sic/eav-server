using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests;

[TestClass]
public class EntityIdFilterTest: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void EntityIdFilter_SingleItem()
    {
        const string itemToFilter = "1023";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListForTests();

        Assert.AreEqual(itemToFilter, ll.First().EntityId.ToString());
    }


    [TestMethod]
    public void EntityIdFilter_NoItems()
    {
        const string itemToFilter = "";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListForTests();

        Assert.AreEqual(0, ll.Count(), "Should return 0 items");
    }

    [TestMethod]
    public void EntityIdFilter_MultipleItems()
    {
        const string itemToFilter = "1011,1023,1050,1003";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListForTests();
        Assert.AreEqual("1011", ll.First().EntityId.ToString(), "Test Light that sorting IS affeted");
        Assert.AreEqual(4, ll.Count(), "Count after filtering");
    }

    [TestMethod]
    public void EntityIdFilter_FilterWithSpaces()
    {
        const string itemToFilter = "1011, 1023 ,1050   ,1003";
        var filtered = CreateFilterForTesting(100, itemToFilter);

        var ll = filtered.ListForTests();
        Assert.AreEqual("1011", ll.First().EntityId.ToString(), "Test Light that sorting IS affeted");
        Assert.AreEqual(4, ll.Count(), "Count after filtering");
    }

    [TestMethod]
    public void EntityIdFilter_CacheKey1()
    {
        // Simple scenario, 1 filter, no special spaces etc.
        const string itemToFilter = "1023";
        var ds = CreateFilterForTesting(100, itemToFilter);

        Assert.AreEqual("EntityIdFilter:NoGuid&EntityIds=1023", ds.CachePartialKey);
        Assert.AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1023", ds.CacheFullKey);
        var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
        Assert.IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");

    }
    [TestMethod]
    public void EntityIdFilter_CacheKeyMulti()
    {
        // Multi-value scenario, no special spaces etc.
        var itemToFilter = "1011,1023,1050,1003";
        var ds = CreateFilterForTesting(100, itemToFilter);

        Assert.AreEqual("EntityIdFilter:NoGuid&EntityIds=1011,1023,1050,1003", ds.CachePartialKey);
        Assert.AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>EntityIdFilter:NoGuid&EntityIds=1011,1023,1050,1003", ds.CacheFullKey);

        // Multi-value scenario, special spaces and trailing comma etc.
        itemToFilter = "1011, 1023  ,   1050,    1003,";
        var partialKey = "EntityIdFilter:NoGuid&EntityIds=1011,1023,1050,1003,";
        ds = CreateFilterForTesting(100, itemToFilter);
        Assert.AreEqual(partialKey, ds.CachePartialKey);
        Assert.AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName>" + partialKey, ds.CacheFullKey);

    }

    public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001);
        var filtered = DsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        filtered.AttachForTests(ds);
        filtered.EntityIds = entityIdsValue;
        return filtered;
    }
}