using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.UnitTests.DataSources;
using ToSic.Eav.TokenEngine.Tests.ValueProvider;

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
            var myConfDs = new EntityIdFilter()
                .Init(ds.Configuration.LookUps);
            //myConfDs.ConfigurationProvider = ds.ConfigurationProvider;
            myConfDs.Attach(ds);
            myConfDs.EntityIds = ItemToFilter;

            testSource.Configuration.Values.Add("SomethingSimple", "Something");
            testSource.Configuration.Values.Add("Token1", new ValueCollectionProvider_Test().OriginalSettingDefaultCat);
            testSource.Configuration.Values.Add("InTestTitle", "[In:Default:EntityTitle]");
            testSource.Configuration.Values.Add("InTestFirstName", "[In:Default:FirstName]");
            testSource.Configuration.Values.Add("InTestBadStream", "[In:InvalidStream:Field]");
            testSource.Configuration.Values.Add("InTestNoKey", "[In:Default]");
            testSource.Configuration.Values.Add("InTestBadKey", "[In:Default:SomeFieldWhichDoesntExist]");
            testSource.Configuration.Values.Add("TestMyConfFirstName", "[In:MyConf:FirstName]");
            testSource.Attach(ds);
            testSource.Attach("MyConf", myConfDs);
            testSource.Init(ds.Configuration.LookUps);//.ConfigTemp.ConfigurationProvider = ds.ConfigurationProvider;
            var y = testSource.List; // must access something to provoke configuration resolving

            Assert.AreEqual("First Name 1001", testSource.Configuration.Values["InTestFirstName"], "Tested in:Default:EntityTitle");
            Assert.AreEqual("", testSource.Configuration.Values["InTestBadStream"], "Testing in-token with invalid stream");
            Assert.AreEqual("", testSource.Configuration.Values["InTestNoKey"], "Testing in-token with missing field");
            Assert.AreEqual("First Name " + ItemToFilter, testSource.Configuration.Values["TestMyConfFirstName"], "MyConf stream First Name");
            Assert.AreEqual("", testSource.Configuration.Values["InTestBadKey"], "Testing in-token with incorrect field name");
        }
    }
}
