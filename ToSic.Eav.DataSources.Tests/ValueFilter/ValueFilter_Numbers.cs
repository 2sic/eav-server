using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterNumbers: EavDataSourceTestBase
    {
        private readonly ValueFilterMaker _valueFilterMaker;

        private const int TestVolume = 10000;
        private readonly ValueFilter _testDataGeneratedOutsideTimer;
        public ValueFilterNumbers()
        {
            _valueFilterMaker = new ValueFilterMaker(this);
            _testDataGeneratedOutsideTimer = _valueFilterMaker.CreateValueFilterForTesting(TestVolume, true);
        }



        #region Number Filters

        [TestMethod]
        public void ValueFilter_FilterNumber()
         =>NumberFilter("Height", (PersonSpecs.MinHeight + 7).ToString(), 181);
        

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

        public void ValueFilter_EntityId()
            => NumberFilter("EntityId", "9818", 9818, "==");

        [TestMethod]
        public void Between()
            => NumberFilter("Height", "175 and 185", 2002, "between");

        [TestMethod]
        public void BetweenNot()
            => NumberFilter("Height", "175 and 185", 10000-2002, "!between");

        public void NumberFilter(string attr, string value, int expected, string operation = null)
        {
            var vf = PrepareNumberFilterDs(attr, value, operation);
            Assert.AreEqual(expected, vf.ListForTests().Count(), "Should find exactly " + expected + " amount people");
        }

        private ValueFilter PrepareNumberFilterDs(string attr, string value, string operation)
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = attr;
            vf.Value = value;
            if (operation != null) vf.Operator = operation;
            return vf;
        }

        #endregion


        [TestMethod]
        public void NumberFilterInvalidOperator()
            => VerifyStreamIsError(PrepareNumberFilterDs("Height", "180", "!!"), 
                ValueFilter.ErrorInvalidOperator);


    }
}
