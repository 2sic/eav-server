using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterNumbers
    {
        private const int TestVolume = 10000;
        private readonly ValueFilter _testDataGeneratedOutsideTimer;
        public ValueFilterNumbers()
        {
            _testDataGeneratedOutsideTimer = ValueFilterString.CreateValueFilterForTesting(TestVolume);
        }



        #region Number Filters

        [TestMethod]
        public void ValueFilter_FilterNumber()
         =>NumberFilter("Height", (DataTableDataSource_Test.MinHeight + 7).ToString(), 181);
        

        [TestMethod]
        public void ValueFilter_FilterNumberNone()
        =>NumberFilter("Height", "72", 0);
        

        [TestMethod]
        public void ValueFilter_FilterNumberEq()
        =>NumberFilter("Height", "182", 182, "==");


        [TestMethod]
        public void ValueFilter_FilterNumberEq2()
            =>NumberFilter("Height", "182", 182, "===");
        

        [TestMethod]
        public void ValueFilter_FilterNumberGt()
            => NumberFilter("Height", "180", 4368, ">");
        

        [TestMethod]
        public void ValueFilter_FilterNumberGtEq()
            => NumberFilter("Height", "180", 4550, ">=");
        

        [TestMethod]
        public void ValueFilter_FilterNumberLt()
            => NumberFilter("Height", "180", 5450, "<");
        

        [TestMethod]
        public void ValueFilter_FilterNumberLtEq()
            => NumberFilter("Height", "180", 5632, "<=");

            [TestMethod]
        public void ValueFilter_FilterNumberNotEq()
            => NumberFilter("Height", "180", 9818, "!=");
       
        public void NumberFilter(string attr, string value, int expected, string operation = null)
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = attr;
            vf.Value = value;
            if (operation != null)
                vf.Operator = operation;
            Assert.AreEqual(expected, vf.List.Count, "Should find exactly " + expected + " amount people");
        }

        #endregion


        // test invalid operator
        [TestMethod]
        [ExpectedException(typeof (System.Exception))]
        public void NumberFilterInvalidOperator()
            => NumberFilter("Height", "180", 5632, "!!");


    }
}
