using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterString
    {
        private const int TestVolume = 10000;
        private readonly ValueFilter _testDataGeneratedOutsideTimer;
        public ValueFilterString()
        {
            _testDataGeneratedOutsideTimer = CreateValueFilterForTesting(TestVolume);
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilter()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = DataTableDataSource_Test.TestCities[0]; // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }
        [TestMethod]
        public void ValueFilter_SimpleTextFilterCIDefault()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = DataTableDataSource_Test.TestCities[0].ToLower(); // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }

        #region String Case Sensitive
        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "===";
            vf.Value = DataTableDataSource_Test.TestCities[0]; // test for the first value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }       

        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithoutResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "===";
            vf.Value = DataTableDataSource_Test.TestCities[0].ToLower(); // test for the first value
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithSomeNulls()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
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
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
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
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = "City";
            vf.Operator = "begins";
            vf.Value = "bu";
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 2500 people with this city");
        }
         [TestMethod]
       public void ValueFilter_SimpleTextFilterBeginsNone()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = "CityMaybeNull";
            vf.Operator = "begins";
            vf.Value = "St.";
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }
        #endregion

        #region String Contains / Not Contains
        [TestMethod]
        public void ValueFilter_SimpleTextFilterContainsWithoutResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "contains";
            vf.Value = "Buchs SG";
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterNotContains()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "!contains";
            vf.Value = "ch";
            Assert.AreEqual(5000, vf.List.Count, "Should find exactly 5000 people with this city");
        }
        #endregion


        [TestMethod]
        public void ValueFilter_SimpleTextFilterWithoutResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = "Daniel";
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }
        
        [TestMethod]
        public void ValueFilter_FilterOnUnexistingProperty()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "ZIPCodeOrSomeOtherNotExistingField";
            vf.Value = "9470"; // test for the first value
            Assert.AreEqual(0, vf.List.Count, "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_FilterFieldSometimesNull()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "CityMaybeNull";
            vf.Value = DataTableDataSource_Test.TestCities[1]; // test for the second value
            Assert.AreEqual(2500, vf.List.Count, "Should find exactly 250 people with this city");
        }
        

        public static ValueFilter CreateValueFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = DataSource.GetDataSource<ValueFilter>(1, 1, ds);
            return filtered;
        }
    }
}
