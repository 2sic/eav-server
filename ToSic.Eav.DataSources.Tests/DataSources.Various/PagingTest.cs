namespace ToSic.Eav.DataSources.Various;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class PagingTest(DataSourcesTstBuilder DsSvc, Generator<DataTablePerson> personTableGenerator)
{
    private readonly int seedId = 1001;

    [Fact]
    public void Paging_BasicPagingPg1On1000Items()
    {
        var ds = CreatePagingForTesting(1000);
        var pgStream = ds.GetStreamTac("Paging")!.ListTac().First();
        Equal(1.ToDecimal(), pgStream.GetTac<int>("PageNumber"));
        Equal(10.ToDecimal(), pgStream.GetTac<int>("PageSize"));
        Equal(100.ToDecimal(), pgStream.GetTac<int>("PageCount"));
        Equal(1000.ToDecimal(), pgStream.GetTac<int>("ItemCount"));

        var result = ds.ListTac();
        Equal(10, result.Count());
        Equal(seedId, result.First().EntityId);
    }

    [Fact]
    public void Paging_BasicPagingPg7On1001Items()
    {
        var ds = CreatePagingForTesting(1001);
        ds.PageNumber = 7;
        var entity = ds.GetStreamTac("Paging")!.ListTac().First();
        Equal(7.ToDecimal(), entity.GetTac<int>("PageNumber"));
        Equal(10.ToDecimal(), entity.GetTac<int>("PageSize"));
        Equal(101.ToDecimal(), entity.GetTac<int>("PageCount"));
        Equal(1001.ToDecimal(), entity.GetTac<int>("ItemCount"));

        var result = ds.ListTac();
        Equal(10, result.Count());
        Equal(seedId + 60, result.First().EntityId);
    }

    [Fact]
    public void Paging_BasicPagingPg50On223Items()
    {
        var ds = CreatePagingForTesting(223);
        ds.PageSize = 50;
        ds.PageNumber = 5;
        var entity = ds.GetStreamTac("Paging")!.ListTac().First();
        Equal(5.ToDecimal(), entity.GetTac<int>("PageNumber"));
        Equal(50.ToDecimal(), entity.GetTac<int>("PageSize"));
        Equal(5.ToDecimal(), entity.GetTac<int>("PageCount"));
        Equal(223.ToDecimal(), entity.GetTac<int>("ItemCount"));

        var result = ds.ListTac();
        Equal(23, result.Count());
        Equal(seedId + 200, result.First().EntityId);
        Equal(seedId + 223 -1, result.Last().EntityId);
    }

    [Fact]
    public void Paging_CacheKeySimple()
    {
        var ds = CreatePagingForTesting(45);
        var expectedPartialKey = "Paging:NoGuid&PageNumber=1&PageSize=10";
        Equal(expectedPartialKey, ds.CachePartialKey);
        Equal("DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName" +
                        ">" + expectedPartialKey, ds.CacheFullKey);
        var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
        True(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
    }

    // to test
    // pickup of settings when it has settings
        
    public Paging CreatePagingForTesting(int testItemsInRootSource)
    {
        var ds = personTableGenerator.New().Generate(testItemsInRootSource, seedId);
        return DsSvc.CreateDataSource<Paging>(ds);
        //return filtered;
    }

}

public static class DecimalHelpers
{
    public static decimal ToDecimal(this Int32 original) => Convert.ToDecimal(original);
}