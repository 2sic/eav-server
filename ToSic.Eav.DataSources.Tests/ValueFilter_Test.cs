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
        private const int testVolume = 10000;
        private ValueFilter TestDataGeneratedOutsideTimer;
        public ValueFilter_Test()
        {
            TestDataGeneratedOutsideTimer = CreateValueFilterForTesting(testVolume);
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilter()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = DataTableDataSource_Test.TestCities[0]; // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }
        [TestMethod]
        public void ValueFilter_SimpleTextFilterCIDefault()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = DataTableDataSource_Test.TestCities[0].ToLower(); // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }

        #region String Case Sensitive
        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithResults()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "===";
            vf.Value = DataTableDataSource_Test.TestCities[0]; // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }       

        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithoutResults()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "===";
            vf.Value = DataTableDataSource_Test.TestCities[0].ToLower(); // test for the first value
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithSomeNulls()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "CityMaybeNull";
            vf.Operator = "===";
            vf.Value = "Grabs"; // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 0 people with this city");
        }

        #endregion

        #region String contains

        [TestMethod]
        public void ValueFilter_SimpleTextFilterContainsWithResults()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "contains";
            vf.Value = "uCHs";
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }
        #endregion

        #region String begins

        [TestMethod]
        public void ValueFilter_SimpleTextFilterBegins()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "begins";
            vf.Value = "bu";
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }
        #endregion
        [TestMethod]
        public void ValueFilter_SimpleTextFilterContainsWithoutResults()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "contains";
            vf.Value = "grabs";
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterWithoutResults()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = "Daniel";
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }
        
        [TestMethod]
        public void ValueFilter_FilterOnUnexistingProperty()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "ZIPCodeOrSomeOtherNotExistingField";
            vf.Value = "9470"; // test for the first value
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_FilterFieldSometimesNull()
        {
            var vf = TestDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "CityMaybeNull";
            vf.Value = DataTableDataSource_Test.TestCities[1]; // test for the second value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 250 people with this city");
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

        #region bool tests

        public void FilterBool(string compareValue, int desiredFinds, int populationRoot)
        {
            var vf = CreateValueFilterForTesting(populationRoot * DataTableDataSource_Test.IsMaleForEveryX); // only every 3rd is male in the demo data
            vf.Attribute = "IsMale";
            vf.Value = compareValue;
            var found = vf.List.Count;
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");
        }

        [TestMethod]
        public void ValueFilter_FilterBool()
        {
            FilterBool(true.ToString(), 82, 82);
        }

        [TestMethod]
        public void ValueFilter_FilterBoolCasing1()
        {
            FilterBool("true", 82, 82);
        }
        [TestMethod]
        public void ValueFilter_FilterBoolCasing2()
        {
            FilterBool("TRUE", 82,82);
        }

        [TestMethod]
        public void ValueFilter_FilterBoolCasing3()
        {
            FilterBool("FALSE", 164, 82);
        }
        #endregion

        public static ValueFilter CreateValueFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = DataSource.GetDataSource<ValueFilter>(1, 1, ds);
            return filtered;
        }
    }
}
