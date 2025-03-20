using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests.LookUps;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class LookUpsTest(DataSourcesTstBuilder dsSvc, DataTablePerson personTableGenerator)
{
    [Fact]
    public void DataTargetValueProvider_General()
    {
        // Assemble a simple source-stream with demo data
        const int ItemsToGenerate = 499;
        const string ItemToFilter = "1023";
        var ds = personTableGenerator.Generate(ItemsToGenerate, 1001);

        var testSource = dsSvc.CreateDataSourceNew<EntityIdFilter>(options: new
        {
            SomethingSimple = "Something",
            Token1 = LookUpTestConstants.OriginalSettingDefaultCat,
            InTestTitle = "[In:Default:EntityTitle]",
            InTestFirstName = "[In:Default:FirstName]",
            InTestBadStream = "[In:InvalidStream:Field]",
            InTestNoKey = "[In:Default]",
            InTestBadKey = "[In:Default:SomeFieldWhichDoesntExist]",
            TestMyConfFirstName = "[In:MyConf:FirstName]",
        }); 
        testSource.EntityIds = "1001";  // needed to ensure 
        var myConfDs = dsSvc.CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
        myConfDs.AttachTac(ds);
        myConfDs.EntityIds = ItemToFilter;

        //testSource.Configuration.Values.Add("SomethingSimple", "Something");
        //testSource.Configuration.Values.Add("Token1", new LookUpEngineTests().OriginalSettingDefaultCat);
        //testSource.Configuration.Values.Add("InTestTitle", "[In:Default:EntityTitle]");
        //testSource.Configuration.Values.Add("InTestFirstName", "[In:Default:FirstName]");
        //testSource.Configuration.Values.Add("InTestBadStream", "[In:InvalidStream:Field]");
        //testSource.Configuration.Values.Add("InTestNoKey", "[In:Default]");
        //testSource.Configuration.Values.Add("InTestBadKey", "[In:Default:SomeFieldWhichDoesntExist]");
        //testSource.Configuration.Values.Add("TestMyConfFirstName", "[In:MyConf:FirstName]");
        testSource.AttachTac(ds);
        testSource.AttachTac("MyConf", myConfDs);

        var y = testSource.ListTac(); // must access something to provoke configuration resolving

        Equal("First Name 1001", testSource.Configuration.Values["InTestFirstName"]);//, "Tested in:Default:EntityTitle");
        Equal("", testSource.Configuration.Values["InTestBadStream"]);//, "Testing in-token with invalid stream");
        Equal("", testSource.Configuration.Values["InTestNoKey"]);//, "Testing in-token with missing field");
        Equal("First Name " + ItemToFilter, testSource.Configuration.Values["TestMyConfFirstName"]);//, "MyConf stream First Name");
        Equal("", testSource.Configuration.Values["InTestBadKey"]);//, "Testing in-token with incorrect field name");
    }
}