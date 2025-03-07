namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[TestClass]
public class PagingTest: TestBaseEavDataSource
{
    private readonly int seedId = 1001;

    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void Paging_BasicPagingPg1On1000Items()
    {
        var ds = CreatePagingForTesting(1000);
        var pgStream = ds["Paging"].ListTac().First();
        AreEqual(1.ToDecimal(), pgStream.GetTac<int>("PageNumber"));
        AreEqual(10.ToDecimal(), pgStream.GetTac<int>("PageSize"));
        AreEqual(100.ToDecimal(), pgStream.GetTac<int>("PageCount"));
        AreEqual(1000.ToDecimal(), pgStream.GetTac<int>("ItemCount"));

        var result = ds.ListTac();
        AreEqual(10, result.Count());
        AreEqual(seedId, result.First().EntityId);
    }

    [TestMethod]
    public void Paging_BasicPagingPg7On1001Items()
    {
        var ds = CreatePagingForTesting(1001);
        ds.PageNumber = 7;
        var pgstream = ds["Paging"].ListTac().First();
        AreEqual(7.ToDecimal(), pgstream.GetTac<int>("PageNumber"));
        AreEqual(10.ToDecimal(), pgstream.GetTac<int>("PageSize"));
        AreEqual(101.ToDecimal(), pgstream.GetTac<int>("PageCount"));
        AreEqual(1001.ToDecimal(), pgstream.GetTac<int>("ItemCount"));

        var result = ds.ListTac();
        AreEqual(10, result.Count());
        AreEqual(seedId + 60, result.First().EntityId);
    }

    [TestMethod]
    public void Paging_BasicPagingPg50On223Items()
    {
        var ds = CreatePagingForTesting(223);
        ds.PageSize = 50;
        ds.PageNumber = 5;
        var pgstream = ds["Paging"].ListTac().First();
        AreEqual(5.ToDecimal(), pgstream.GetTac<int>("PageNumber"));
        AreEqual(50.ToDecimal(), pgstream.GetTac<int>("PageSize"));
        AreEqual(5.ToDecimal(), pgstream.GetTac<int>("PageCount"));
        AreEqual(223.ToDecimal(), pgstream.GetTac<int>("ItemCount"));

        var result = ds.ListTac();
        AreEqual(23, result.Count());
        AreEqual(seedId + 200, result.First().EntityId);
        AreEqual(seedId + 223 -1, result.Last().EntityId);
    }

    [TestMethod]
    public void Paging_CacheKeySimple()
    {
        var ds = CreatePagingForTesting(45);
        var expectedPartialKey = "Paging:NoGuid&PageNumber=1&PageSize=10";
        AreEqual(expectedPartialKey, ds.CachePartialKey);
        AreEqual("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName" +
                        ">" + expectedPartialKey, ds.CacheFullKey);
        var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
        IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
    }

    // to test
    // pickup of settings when it has settings
        
    public Paging CreatePagingForTesting(int testItemsInRootSource)
    {
        var ds = new DataTablePerson(this).Generate(testItemsInRootSource, seedId);
        return DsSvc.CreateDataSource<Paging>(ds);
        //return filtered;
    }

}

public static class DecimalHelpers
{
    public static decimal ToDecimal(this Int32 original) => Convert.ToDecimal(original);
}