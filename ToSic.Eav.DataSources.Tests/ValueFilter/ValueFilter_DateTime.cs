using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterDateTime: TestBaseDiEavFullAndDb
    {
        private const int TestVolume = 10000;
        private readonly ValueFilter _testDataGeneratedOutsideTimer;
        public ValueFilterDateTime()
        {
            var valueFilterMaker = new ValueFilterMaker(this);
            _testDataGeneratedOutsideTimer = valueFilterMaker.CreateValueFilterForTesting(TestVolume, true);
        }



        #region DateTime Filters

        [DataRow("Birthdate", "1990-01-01", 2, null, "EqIso")]
        [DataRow("Birthdate", "4/4/1903", 2, null, "EqUsa")]
        [DataRow("Birthdate", "6.6.1905", 2, null, "EqEurope")]
        [DataRow("Modified", "6.6.1905", 0, null, "DateModifiedZero")]
        [DataRow("Modified", "2100-01-01", 10000, "<", "DateModifiedOlder")]
        [DataRow("Modified", "0000-01-01 and 2100-01-01", 10000, "Between", "DateModifiedBetween")]
        [DataRow("Birthdate", "24.8.1997", 1124, ">", "Gt")]// this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 10000 - 1127, "<", "Lt")]// this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 1127, ">=", "GtEq")] // this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 10000 - 1124, "<=", "LtEq")] // this is one of the generated dates

        [DataRow("Birthdate", "24.8.1997", 3, "===", "Eq1997")] // this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 10000 - 3, "!=", "NotEq1997")] // this is one of the generated dates
        [DataRow("Birthdate", "1.1.1995 and 31.12.2000", 546, "between", "Between")] // this is one of the generated dates
        [DataRow("Birthdate", "1.1.1995 and 31.12.2000", 10000 - 546, "!between", "BetweenNot")] // this is one of the generated dates
        [DataTestMethod]
        public void DateTimeFilter(string attr, string value, int expected, string operation, string name)
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = attr;
            vf.Value = value;
            if (operation != null)
                vf.Operator = operation;
            Assert.AreEqual(expected, vf.ListForTests().Count(), "Should find exactly " + expected + " amount people");
        }
        

        #endregion

        #region DateTime Null filter

        [DataRow("1.1.1995 and 31.12.2000", 273, "between", "NullBetween")]
        [DataRow("1.1.1995 and 31.12.2000", 9727, "!between", "NullBetweenNot")]
        [DataRow(" and 31.12.2000", 9636, "between", "NullBetweenNullAndReal")]
        [DataRow("", 10000 / 2, "==", "Null ==")]
        [DataRow("", 10000 / 2, "!=", "Null !=")]
        [DataRow("24.8.1997", 546, ">", "Null >")]
        [DataRow("24.8.1997", 9454, "<", "Null <")]
        [DataRow("24.8.1997", 546, ">=", "Null >=")]
        [DataRow("24.8.1997", 9454, "<=", "Null <=")]
        [DataTestMethod]
        public void FilterNullBDay(string value, int expected, string operation, string name = null)
            => DateTimeFilter("BirthdateMaybeNull", value, expected, operation, null);

        #endregion


        // test invalid operator
        [TestMethod]
        [ExpectedException(typeof (System.Exception))]
        public void NumberFilterInvalidOperator()
            => DateTimeFilter("Birthdate", "180", 5632, "!!", null);


    }
}
