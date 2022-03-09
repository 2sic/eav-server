using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSources.CompareOperators;

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
        [DataRow("Modified", "2100-01-01", 10000, OpLt, "DateModifiedOlder")]
        [DataRow("Modified", "0000-01-01 and 2100-01-01", 10000, "Between", "DateModifiedBetween")]
        [DataRow("Birthdate", "24.8.1997", 1124, OpGt, "Gt")]// this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 10000 - 1127, OpLt, "Lt")]// this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 1127, OpGtEquals, "GtEq")] // this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 10000 - 1124, OpLtEquals, "LtEq")] // this is one of the generated dates

        [DataRow("Birthdate", "24.8.1997", 3, OpExactly, "Eq1997")] // this is one of the generated dates
        [DataRow("Birthdate", "24.8.1997", 10000 - 3, OpNotEquals, "NotEq1997")] // this is one of the generated dates
        [DataRow("Birthdate", "1.1.1995 and 31.12.2000", 546, OpBetween, "Between")] // this is one of the generated dates
        [DataRow("Birthdate", "1.1.1995 and 31.12.2000", 10000 - 546, OpNotBetween, "BetweenNot")] // this is one of the generated dates
        [DataTestMethod]
        public void DateTimeFilter(string attr, string value, int expected, string operation, string name)
        {
            var vf = PrepareDateTimeFilterDs(attr, value, operation);
            Assert.AreEqual(expected, vf.ListForTests().Count(), "Should find exactly " + expected + " amount people");
        }

        private ValueFilter PrepareDateTimeFilterDs(string attr, string value, string operation)
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = attr;
            vf.Value = value;
            if (operation != null) vf.Operator = operation;
            return vf;
        }

        #endregion

        #region DateTime Null filter

        [DataRow("1.1.1995 and 31.12.2000", 273, OpBetween, "NullBetween")]
        [DataRow("1.1.1995 and 31.12.2000", 9727, OpNotBetween, "NullBetweenNot")]
        [DataRow(" and 31.12.2000", 9636, OpBetween, "NullBetweenNullAndReal")]
        [DataRow("", 10000 / 2, OpEquals, "Null ==")]
        [DataRow("", 10000 / 2, OpNotEquals, "Null !=")]
        [DataRow("24.8.1997", 546, OpGt, "Null >")]
        [DataRow("24.8.1997", 9454, OpLt, "Null <")]
        [DataRow("24.8.1997", 546, OpGtEquals, "Null >=")]
        [DataRow("24.8.1997", 9454, OpLtEquals, "Null <=")]
        [DataTestMethod]
        public void FilterNullBDay(string value, int expected, string operation, string name = null)
            => DateTimeFilter("BirthdateMaybeNull", value, expected, operation, null);

        #endregion


        [TestMethod]
        public void NumberFilterInvalidOperator()
            => DataSourceErrors.VerifyStreamIsError(PrepareDateTimeFilterDs("Birthdate", "180", "!!"),
                ErrorInvalidOperator);


    }
}
