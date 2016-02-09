using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.UnitTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilter_Test
    {
        [TestMethod]
        public void ValueFilter_SimpleTextFilter()
        {
            var vf = CreateValueFilterForTesting(1000);
            vf.Attribute = "City";
            vf.Value = DataTableDataSource_Test.TestCities[0]; // test for the first value
            Assert.AreEqual(250, vf.List.Count, "Should find exactly 250 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterWithoutResults()
        {
            var vf = CreateValueFilterForTesting(1000);
            vf.Attribute = "City";
            vf.Value = "Daniel";
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }
        
        [TestMethod]
        public void ValueFilter_FilterOnUnexistingProperty()
        {
            var vf = CreateValueFilterForTesting(1000);
            vf.Attribute = "ZIPCodeOrSomeOtherNotExistingField";
            vf.Value = "9470"; // test for the first value
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_FilterFieldSometimesNull()
        {
            var vf = CreateValueFilterForTesting(1000);
            vf.Attribute = "CityMaybeNull";
            vf.Value = DataTableDataSource_Test.TestCities[1]; // test for the second value
            Assert.AreEqual(250, vf.List.Count, "Should find exactly 250 people with this city");
        }

        [TestMethod]
        public void ValueFilter_FilterNumber()
        {
            var desiredFinds = 20;
            var vf = CreateValueFilterForTesting(DataTableDataSource_Test.HeightVar * desiredFinds);
            vf.Attribute = "Height";
            vf.Value = (DataTableDataSource_Test.MinHeight + 7).ToString();
            Assert.AreEqual(desiredFinds, vf.List.Count, "Should find exactly this amount people");
        }

        [TestMethod]
        public void ValueFilter_FilterBool()
        {
            var desiredFinds = 82;
            var vf = CreateValueFilterForTesting(desiredFinds * DataTableDataSource_Test.IsMaleForEveryX); // only every 3rd is male in the demo data
            vf.Attribute = "IsMale";
            vf.Value = true.ToString();
            Assert.AreEqual(desiredFinds, vf.List.Count, "Should find exactly this amount people");
        }
        
        public static ValueFilter CreateValueFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = DataSource.GetDataSource<ValueFilter>(1, 1, ds);
            return filtered;
        }
    }
}
