using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.UnitTests.ValueProvider
{
    [TestClass]
    public class DataTargetValueProvider_Test
    {
        [TestMethod]
        public void DataTargetValueProvider_General()
        {
            var testSource = new EntityIdFilter();
            testSource.EntityIds = "1001";  // needed to ensure 
            // Assemble a simple source-stream with demo data
            const int ItemsToGenerate = 499;
            const string ItemToFilter = "1023";
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(ItemsToGenerate, 1001);
            var myConfDs = new EntityIdFilter();
            myConfDs.ConfigurationProvider = ds.ConfigurationProvider;
            myConfDs.Attach(ds);
            myConfDs.EntityIds = ItemToFilter;

            testSource.Configuration.Add("SomethingSimple", "Something");
            testSource.Configuration.Add("Token1", new ValueCollectionProvider_Test().OriginalSettingDefaultCat);
            testSource.Configuration.Add("InTestTitle", "[In:Default:EntityTitle]");
            testSource.Configuration.Add("InTestFirstName", "[In:Default:FirstName]");
            testSource.Configuration.Add("InTestBadStream", "[In:InvalidStream:Field]");
            testSource.Configuration.Add("InTestNoKey", "[In:Default]");
            testSource.Configuration.Add("InTestBadKey", "[In:Default:SomeFieldWhichDoesntExist]");
            testSource.Configuration.Add("TestMyConfFirstName", "[In:MyConf:FirstName]");
            testSource.Attach(ds);
            testSource.Attach("MyConf", myConfDs);
            testSource.ConfigurationProvider = ds.ConfigurationProvider;
            var y = testSource.List; // must access something to provoke configuration resolving

            Assert.AreEqual("First Name 1001", testSource.Configuration["InTestFirstName"], "Tested in:Default:EntityTitle");
            Assert.AreEqual("", testSource.Configuration["InTestBadStream"], "Testing in-token with invalid stream");
            Assert.AreEqual("", testSource.Configuration["InTestNoKey"], "Testing in-token with missing field");
            Assert.AreEqual("First Name " + ItemToFilter, testSource.Configuration["TestMyConfFirstName"], "MyConf stream First Name");
            Assert.AreEqual("", testSource.Configuration["InTestBadKey"], "Testing in-token with incorrect field name");
        }
    }
}
