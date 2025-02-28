using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.LookUps;

[TestClass]
public class LookUpsTest: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void DataTargetValueProvider_General()
    {
        // Assemble a simple source-stream with demo data
        const int ItemsToGenerate = 499;
        const string ItemToFilter = "1023";
        var ds = new DataTablePerson(this).Generate(ItemsToGenerate, 1001);

        var testSource = DsSvc.CreateDataSourceNew<EntityIdFilter>(options: new
        {
            SomethingSimple = "Something",
            Token1 = new LookUpEngineTests().OriginalSettingDefaultCat,
            InTestTitle = "[In:Default:EntityTitle]",
            InTestFirstName = "[In:Default:FirstName]",
            InTestBadStream = "[In:InvalidStream:Field]",
            InTestNoKey = "[In:Default]",
            InTestBadKey = "[In:Default:SomeFieldWhichDoesntExist]",
            TestMyConfFirstName = "[In:MyConf:FirstName]",
        }); 
        testSource.EntityIds = "1001";  // needed to ensure 
        var myConfDs = DsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        myConfDs.AttachForTests(ds);
        myConfDs.EntityIds = ItemToFilter;

        //testSource.Configuration.Values.Add("SomethingSimple", "Something");
        //testSource.Configuration.Values.Add("Token1", new LookUpEngineTests().OriginalSettingDefaultCat);
        //testSource.Configuration.Values.Add("InTestTitle", "[In:Default:EntityTitle]");
        //testSource.Configuration.Values.Add("InTestFirstName", "[In:Default:FirstName]");
        //testSource.Configuration.Values.Add("InTestBadStream", "[In:InvalidStream:Field]");
        //testSource.Configuration.Values.Add("InTestNoKey", "[In:Default]");
        //testSource.Configuration.Values.Add("InTestBadKey", "[In:Default:SomeFieldWhichDoesntExist]");
        //testSource.Configuration.Values.Add("TestMyConfFirstName", "[In:MyConf:FirstName]");
        testSource.AttachForTests(ds);
        testSource.AttachForTests("MyConf", myConfDs);

        var y = testSource.ListForTests(); // must access something to provoke configuration resolving

        Assert.AreEqual("First Name 1001", testSource.Configuration.Values["InTestFirstName"], "Tested in:Default:EntityTitle");
        Assert.AreEqual("", testSource.Configuration.Values["InTestBadStream"], "Testing in-token with invalid stream");
        Assert.AreEqual("", testSource.Configuration.Values["InTestNoKey"], "Testing in-token with missing field");
        Assert.AreEqual("First Name " + ItemToFilter, testSource.Configuration.Values["TestMyConfFirstName"], "MyConf stream First Name");
        Assert.AreEqual("", testSource.Configuration.Values["InTestBadKey"], "Testing in-token with incorrect field name");
    }
}