namespace ToSic.Eav.DataSourceTests.ExternalData;

[TestClass]
public class DataTableTst: TestBaseEavDataSource
{

    [TestMethod]
    public void DataSource_Create_GeneralTest()
    {
        const int itemsToGenerate = 499;
        var ds = new DataTablePerson(this).Generate(itemsToGenerate);
        IsTrue(ds.InTac().Count == 0, "In count should be 0");
        IsTrue(ds.Out.Count == 1, "Out count should be 1");
        var defaultOut = ds[DataSourceConstants.StreamDefaultName];
        IsTrue(defaultOut != null);
        try
        {
            // ReSharper disable once UnusedVariable
            var x = ds["Something"];
            Fail("Access to another out should fail");
        }
        catch { }
        IsTrue(defaultOut.ListTac().Count() == itemsToGenerate);
    }

    [TestMethod]
    public void DataTable_CacheKey()
    {
        const int itemsToGenerate = 499;
        var ds = new DataTablePerson(this).Generate(itemsToGenerate);

        //var expKey = "DataTable:NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person";
        var expKey = "DataTable:NoGuid&ContentType=Person&EntityIdField=entityid&ModifiedField=InternalModified&TitleField=FullName";
        AreEqual(expKey, ds.CachePartialKey);
        AreEqual(expKey, ds.CacheFullKey);
        var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
        IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
    }

    [TestMethod]
    public void DataTable_DefaultTitleField()
    {
        const int itemsToGenerate = 25;
        var ds = new DataTableTrivial(this).Generate(itemsToGenerate);

        AreEqual(25, ds.ListTac().Count());
        var first = ds.ListTac().FirstOrDefault();
        AreEqual("Daniel Mettler", first.GetBestTitle());
    }


}