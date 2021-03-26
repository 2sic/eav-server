using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;

namespace ToSic.Eav.DataSourceTests.LookUps
{
    [TestClass]
    public class LookUpsTest
    {
        [TestMethod]
        public void DataTargetValueProvider_General()
        {
            var testSource = new EntityIdFilter();
            testSource.EntityIds = "1001";  // needed to ensure 
            // Assemble a simple source-stream with demo data
            const int ItemsToGenerate = 499;
            const string ItemToFilter = "1023";
            var ds = DataTablePerson.Generate(ItemsToGenerate, 1001);
            var myConfDs = new EntityIdFilter()
                .Init(ds.Configuration.LookUpEngine);
            //myConfDs.ConfigurationProvider = ds.ConfigurationProvider;
            myConfDs.AttachForTests(ds);
            myConfDs.EntityIds = ItemToFilter;

            testSource.Configuration.Values.Add("SomethingSimple", "Something");
            testSource.Configuration.Values.Add("Token1", new LookUpEngineTests().OriginalSettingDefaultCat);
            testSource.Configuration.Values.Add("InTestTitle", "[In:Default:EntityTitle]");
            testSource.Configuration.Values.Add("InTestFirstName", "[In:Default:FirstName]");
            testSource.Configuration.Values.Add("InTestBadStream", "[In:InvalidStream:Field]");
            testSource.Configuration.Values.Add("InTestNoKey", "[In:Default]");
            testSource.Configuration.Values.Add("InTestBadKey", "[In:Default:SomeFieldWhichDoesntExist]");
            testSource.Configuration.Values.Add("TestMyConfFirstName", "[In:MyConf:FirstName]");
            testSource.AttachForTests(ds);
            testSource.AttachForTests("MyConf", myConfDs);
            testSource.Init(ds.Configuration.LookUpEngine);//.ConfigTemp.ConfigurationProvider = ds.ConfigurationProvider;
            var y = testSource.List; // must access something to provoke configuration resolving

            Assert.AreEqual("First Name 1001", testSource.Configuration.Values["InTestFirstName"], "Tested in:Default:EntityTitle");
            Assert.AreEqual("", testSource.Configuration.Values["InTestBadStream"], "Testing in-token with invalid stream");
            Assert.AreEqual("", testSource.Configuration.Values["InTestNoKey"], "Testing in-token with missing field");
            Assert.AreEqual("First Name " + ItemToFilter, testSource.Configuration.Values["TestMyConfFirstName"], "MyConf stream First Name");
            Assert.AreEqual("", testSource.Configuration.Values["InTestBadKey"], "Testing in-token with incorrect field name");
        }
    }
}
