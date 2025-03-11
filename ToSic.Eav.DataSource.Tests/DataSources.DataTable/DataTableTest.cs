using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.StartupTests;

namespace ToSic.Eav.DataSources.DataTable;

[Startup(typeof(StartupTestsEavCoreAndDataSources))]
public class DataTableTest(DataTablePerson dataTablePerson, DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{

    [Fact]
    public void DataSource_Create_GeneralTest()
    {
        const int itemsToGenerate = 499;
        var ds = dataTablePerson.Generate(itemsToGenerate);
        True(ds.InTac().Count == 0, "In count should be 0");
        True(ds.Out.Count == 1, "Out count should be 1");
        var defaultOut = ds[DataSourceConstants.StreamDefaultName];
        True(defaultOut != null);
        try
        {
            // ReSharper disable once UnusedVariable
            var x = ds["Something"];
            Fail("Access to another out should fail");
        }
        catch { }
        True(defaultOut.ListTac().Count() == itemsToGenerate);
    }

    [Fact]
    public void DataTable_CacheKey()
    {
        const int itemsToGenerate = 499;
        var ds = dataTablePerson.Generate(itemsToGenerate);

        //var expKey = "DataTable:NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person";
        var expKey = "DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName";
        Equal(expKey, ds.CachePartialKey);
        Equal(expKey, ds.CacheFullKey);
        var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
        True(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
    }

    [Fact]
    public void DataTable_DefaultTitleField()
    {
        const int itemsToGenerate = 25;
        var ds = new DataTableTrivial(dsSvc, dataBuilder).Generate(itemsToGenerate);

        Equal(25, ds.ListTac().Count());
        var first = ds.ListTac().FirstOrDefault();
        Equal("Daniel Mettler", first.GetBestTitle());
    }


}