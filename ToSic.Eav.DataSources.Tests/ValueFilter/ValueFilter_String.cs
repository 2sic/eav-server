using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.Apps;
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
            vf.Value = DataTableDataSourceTest.TestCities[0]; // test for the first value
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 2500 people with this city");
        }
        [TestMethod]
        public void ValueFilter_SimpleTextFilterCIDefault()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = DataTableDataSourceTest.TestCities[0].ToLower(); // test for the first value
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 2500 people with this city");
        }

        #region String Case Sensitive
        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "===";
            vf.Value = DataTableDataSourceTest.TestCities[0]; // test for the first value
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 2500 people with this city");
        }       

        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithoutResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "===";
            vf.Value = DataTableDataSourceTest.TestCities[0].ToLower(); // test for the first value
            Assert.AreEqual(0, vf.List.Count(), "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterCSWithSomeNulls()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "CityMaybeNull";
            vf.Operator = "===";
            vf.Value = "Grabs"; // test for the first value
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 0 people with this city");
        }

        #endregion

        #region "all"
        [TestMethod]
        public void ValueFilter_All()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = "City";
            vf.Operator = "all";
            vf.Value = "uCHs";
            Assert.AreEqual(10000, vf.List.Count(), "Should find exactly 10000 people with this city");
        }
        #endregion

        #region String contains

        [TestMethod]
        public void ValueFilter_TextContainsWithResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "contains";
            vf.Value = "uCHs";
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 2500 people with this city");
        }

        #endregion

        #region Take-tests and "all"

        [TestMethod]
       public void ValueFilter_TextContainsOneOnly()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "contains";
            vf.Value = "uCHs";
            vf.Take = "5";
            Assert.AreEqual(5, vf.List.Count(), "Should find exactly 5 people with this city");
        }
        [TestMethod]
       public void ValueFilter_TakeContainsCH1000()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "contains";
            vf.Value = "CH";
            vf.Take = "1000";
            Assert.AreEqual(1000, vf.List.Count(), "Should find exactly 5 people with this city");
        }
        [TestMethod]
       public void ValueFilter_TakeAll10000()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "all";
            vf.Value = "uCHs";
            vf.Take = "10000";
            Assert.AreEqual(10000, vf.List.Count(), "Should find exactly 5 people with this city");
        }        

        [TestMethod]
       public void ValueFilter_TakeAll90000()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "all";
            vf.Value = "uCHs";
            vf.Take = "90000";
            Assert.AreEqual(10000, vf.List.Count(), "Should find exactly 5 people with this city");
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
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 2500 people with this city");
        }
         [TestMethod]
       public void ValueFilter_SimpleTextFilterBeginsNone()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = "CityMaybeNull";
            vf.Operator = "begins";
            vf.Value = "St.";
            Assert.AreEqual(0, vf.List.Count(), "Should find exactly 0 people with this city");
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
            Assert.AreEqual(0, vf.List.Count(), "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_SimpleTextFilterNotContains()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Operator = "!contains";
            vf.Value = "ch";
            Assert.AreEqual(5000, vf.List.Count(), "Should find exactly 5000 people with this city");
        }
        #endregion


        [TestMethod]
        public void ValueFilter_SimpleTextFilterWithoutResults()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "City";
            vf.Value = "Daniel";
            Assert.AreEqual(0, vf.List.Count(), "Should find exactly 0 people with this city");
        }
        
        [TestMethod]
        public void ValueFilter_FilterOnUnexistingProperty()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "ZIPCodeOrSomeOtherNotExistingField";
            vf.Value = "9470"; // test for the first value
            Assert.AreEqual(0, vf.List.Count(), "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_FilterFieldSometimesNull()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueFilterForTesting(testVolume);
            vf.Attribute = "CityMaybeNull";
            vf.Value = DataTableDataSourceTest.TestCities[1]; // test for the second value
            Assert.AreEqual(2500, vf.List.Count(), "Should find exactly 250 people with this city");
        }


        #region Fallback Tests
        [TestMethod]
        public void ValueFilter_WithoutFallback()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = "City";
            // vf.Operator = "==";
            vf.Value = "inexisting city";
            Assert.AreEqual(0, vf.List.Count(), "Should find exactly 0 people with this city");
        }

        [TestMethod]
        public void ValueFilter_WithFallback()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = "City";
            // vf.Operator = "==";
            vf.Value = "inexisting city";
            
            // attach fallback to give all if no match
            vf.In.Add(Constants.FallbackStreamName, vf.In[Constants.DefaultStreamName]);
            Assert.AreEqual(TestVolume, vf.List.Count(), "Should find exactly 0 people with this city");
        }
        #endregion

        public static  ValueFilter CreateValueFilterForTesting(int testItemsInRootSource)
        {
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = new DataSource(null).GetDataSource<ValueFilter>(new AppIdentity(1, 1), ds);
            return filtered;
        }
    }
}
