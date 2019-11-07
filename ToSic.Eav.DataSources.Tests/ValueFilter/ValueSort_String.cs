using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // 1. Tests with standard fields like EntityTitle
    // 2. Tests with multiple fileds like City,Name
    // Later: Create tests with language-parameters as well, as these tests ignore the language and always use default
    

    [TestClass]
    public class ValueSort_String
    {
        private const int TestVolume = 30, InitialId = 1001;
        private const string City = "City";
        private readonly ValueSort _testDataGeneratedOutsideTimer;
        public ValueSort_String()
        {
            _testDataGeneratedOutsideTimer = ValueSortShared.GeneratePersonSourceWithDemoData(TestVolume, InitialId);
        }

        [TestMethod]
        public void ValueSort_Unused()
        {
            var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
            var listOut = vf.List.ToList();
            var listIn = vf.In["Default"].List.ToList();
            CollectionAssert.AreEqual(listOut, listIn, "Lists should be the same if no criteria applied");
        }

        [TestMethod]
        public void ValueSort_SortFieldOnly_City()
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attributes = City;
            var result = vf.List.ToList();
            // check that each following city is same or larger...
            ValidateFieldIsSorted(result, City, true);
        }

        [TestMethod]
        public void ValueSort_SortFieldAndAsc_City()
        {
            TestSortFieldAndDirection(City, "asc", true);
        }

        private void TestSortFieldAndDirection(string field, string direction, bool testAscending)
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attributes = field;
            vf.Directions = direction;
            var result = vf.List.ToList();
            // check that each following city is same or larger...
            ValidateFieldIsSorted(result, field, testAscending);
        }

        [TestMethod]
        public void ValueSort_SortFieldAndA_City()
        {
            TestSortFieldAndDirection(City, "a", true);
        }
        [TestMethod]
        public void ValueSort_SortFieldAndDir1_City()
        {
            TestSortFieldAndDirection(City, "1", true);
        }
        public void ValueSort_SortFieldAndGt_City()
        {
            TestSortFieldAndDirection(City, "<", true);
        }
        [TestMethod]
        public void ValueSort_SortFieldAndDesc_City()
        {
            TestSortFieldAndDirection(City, "desc", false);
        }
        [TestMethod]
        public void ValueSort_SortFieldAndD_City()
        {
            TestSortFieldAndDirection(City, "d", false);
        }
        public void ValueSort_SortFieldAndDCase_City()
        {
            TestSortFieldAndDirection(City, "D", false);
        }
        [TestMethod]
        public void ValueSort_SortFieldAnd0_City()
        {
            TestSortFieldAndDirection(City, "0", false);
        }
        [TestMethod]
        public void ValueSort_SortFieldAndLt_City()
        {
            TestSortFieldAndDirection(City, ">", false);
        }

        private void ValidateFieldIsSorted(List<IEntity> list, string field, bool asc)
        {
            var previous = list.First().GetBestValue(field).ToString();
            foreach (var entity in list)
            {
                var next = entity.GetBestValue(field).ToString();
                var comp = string.Compare(previous, next, StringComparison.Ordinal);
                if (asc)
                    Assert.IsTrue(comp < 1, "new " + field + " " + next + " should be = or larger than prev " + previous);
                else
                    Assert.IsTrue(comp > -1, "new " + field + " " + next + " should be = or smaller than prev " + previous);
                previous = next;
            }            
        }

        //[TestMethod]
        //public void ValueSort_SimpleTextFilterCIDefault()
        //{
        //    var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
        //    vf.Attribute = "City";
        //    vf.Value = DataTableDataSourceTest.TestCities[0].ToLower(); // test for the first value
        //    Assert.AreEqual(2500, vf.LightList.Count(), "Should find exactly 2500 people with this city");
        //}

       // #region String Case Sensitive
       // [TestMethod]
       // public void ValueSort_SimpleTextFilterCSWithResults()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "===";
       //     vf.Value = DataTableDataSourceTest.TestCities[0]; // test for the first value
       //     Assert.AreEqual(2500, vf.LightList.Count(), "Should find exactly 2500 people with this city");
       // }       

       // [TestMethod]
       // public void ValueSort_SimpleTextFilterCSWithoutResults()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "===";
       //     vf.Value = DataTableDataSourceTest.TestCities[0].ToLower(); // test for the first value
       //     Assert.AreEqual(0, vf.LightList.Count(), "Should find exactly 0 people with this city");
       // }

       // [TestMethod]
       // public void ValueSort_SimpleTextFilterCSWithSomeNulls()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "CityMaybeNull";
       //     vf.Operator = "===";
       //     vf.Value = "Grabs"; // test for the first value
       //     Assert.AreEqual(2500, vf.LightList.Count(), "Should find exactly 0 people with this city");
       // }

       // #endregion

       // #region "all"
       // [TestMethod]
       // public void ValueSort_All()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;
       //     vf.Attribute = "City";
       //     vf.Operator = "all";
       //     vf.Value = "uCHs";
       //     Assert.AreEqual(10000, vf.LightList.Count(), "Should find exactly 10000 people with this city");
       // }
       // #endregion

       // #region String contains

       // [TestMethod]
       // public void ValueSort_TextContainsWithResults()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "contains";
       //     vf.Value = "uCHs";
       //     Assert.AreEqual(2500, vf.LightList.Count(), "Should find exactly 2500 people with this city");
       // }

       // #endregion

       // #region Take-tests and "all"

       // [TestMethod]
       //public void ValueSort_TextContainsOneOnly()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "contains";
       //     vf.Value = "uCHs";
       //     vf.Take = "5";
       //     Assert.AreEqual(5, vf.LightList.Count(), "Should find exactly 5 people with this city");
       // }
       // [TestMethod]
       //public void ValueSort_TakeContainsCH1000()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "contains";
       //     vf.Value = "CH";
       //     vf.Take = "1000";
       //     Assert.AreEqual(1000, vf.LightList.Count(), "Should find exactly 5 people with this city");
       // }
       // [TestMethod]
       //public void ValueSort_TakeAll10000()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "all";
       //     vf.Value = "uCHs";
       //     vf.Take = "10000";
       //     Assert.AreEqual(10000, vf.LightList.Count(), "Should find exactly 5 people with this city");
       // }        

       // [TestMethod]
       //public void ValueSort_TakeAll90000()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "all";
       //     vf.Value = "uCHs";
       //     vf.Take = "90000";
       //     Assert.AreEqual(10000, vf.LightList.Count(), "Should find exactly 5 people with this city");
       // }        
       // #endregion

       // #region String begins

       // [TestMethod]
       // public void ValueSort_SimpleTextFilterBegins()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;
       //     vf.Attribute = "City";
       //     vf.Operator = "begins";
       //     vf.Value = "bu";
       //     Assert.AreEqual(2500, vf.LightList.Count(), "Should find exactly 2500 people with this city");
       // }
       //  [TestMethod]
       //public void ValueSort_SimpleTextFilterBeginsNone()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;
       //     vf.Attribute = "CityMaybeNull";
       //     vf.Operator = "begins";
       //     vf.Value = "St.";
       //     Assert.AreEqual(0, vf.LightList.Count(), "Should find exactly 0 people with this city");
       // }
       // #endregion

       // #region String Contains / Not Contains
       // [TestMethod]
       // public void ValueSort_SimpleTextFilterContainsWithoutResults()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "contains";
       //     vf.Value = "Buchs SG";
       //     Assert.AreEqual(0, vf.LightList.Count(), "Should find exactly 0 people with this city");
       // }

       // [TestMethod]
       // public void ValueSort_SimpleTextFilterNotContains()
       // {
       //     var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
       //     vf.Attribute = "City";
       //     vf.Operator = "!contains";
       //     vf.Value = "ch";
       //     Assert.AreEqual(5000, vf.LightList.Count(), "Should find exactly 5000 people with this city");
       // }
       // #endregion


        //[TestMethod]
        //public void ValueSort_SimpleTextFilterWithoutResults()
        //{
        //    var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
        //    vf.Attribute = "City";
        //    vf.Value = "Daniel";
        //    Assert.AreEqual(0, vf.LightList.Count(), "Should find exactly 0 people with this city");
        //}
        
        //[TestMethod]
        //public void ValueSort_FilterOnUnexistingProperty()
        //{
        //    var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
        //    vf.Attribute = "ZIPCodeOrSomeOtherNotExistingField";
        //    vf.Value = "9470"; // test for the first value
        //    Assert.AreEqual(0, vf.LightList.Count(), "Should find exactly 0 people with this city");
        //}

        //[TestMethod]
        //public void ValueSort_FilterFieldSometimesNull()
        //{
        //    var vf = _testDataGeneratedOutsideTimer;// CreateValueSortForTesting(testVolume);
        //    vf.Attribute = "CityMaybeNull";
        //    vf.Value = DataTableDataSourceTest.TestCities[1]; // test for the second value
        //    Assert.AreEqual(2500, vf.LightList.Count(), "Should find exactly 250 people with this city");
        //}
        
    }
}
