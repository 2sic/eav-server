using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterDateTime
    {
        private const int TestVolume = 10000;
        private readonly ValueFilter _testDataGeneratedOutsideTimer;
        public ValueFilterDateTime()
        {
            _testDataGeneratedOutsideTimer = ValueFilterString.CreateValueFilterForTesting(TestVolume);
        }



        #region DateTime Filters

        [TestMethod]
        public void EqIso()
            => DateTimeFilter("Birthdate", "1990-01-01", 2);
        
        [TestMethod]
        public void EqUsa()
            => DateTimeFilter("Birthdate", "4/4/1903", 2);

        [TestMethod]
        public void EqEurope()
            => DateTimeFilter("Birthdate", "6.6.1905", 2);

        [TestMethod]
        public void ValueFilter_DateModifiedZero()
            => DateTimeFilter("Modified", "6.6.1905", 0);

        [TestMethod]
        public void ValueFilter_DateModifiedOlder()
            => DateTimeFilter("Modified", "2100-01-01", 10000, "<");
        [TestMethod]
        public void ValueFilter_DateModifiedBetween()
            => DateTimeFilter("Modified", "0000-01-01 and 2100-01-01", 10000, "Between");

        [TestMethod]
        public void Gt()
            => DateTimeFilter("Birthdate", "24.8.1997", 1124, ">");// this is one of the generated dates

        [TestMethod]
        public void Lt()
            => DateTimeFilter("Birthdate", "24.8.1997", 10000-1127, "<");// this is one of the generated dates

        [TestMethod]
        public void GtEq()
            => DateTimeFilter("Birthdate", "24.8.1997", 1127, ">="); // this is one of the generated dates

        [TestMethod]
        public void LtEq()
            => DateTimeFilter("Birthdate", "24.8.1997", 10000-1124, "<="); // this is one of the generated dates

        [TestMethod]
        public void Eq1997()
            => DateTimeFilter("Birthdate", "24.8.1997", 3, "==="); // this is one of the generated dates

        [TestMethod]
        public void NotEq1997()
            => DateTimeFilter("Birthdate", "24.8.1997", 10000-3, "!="); // this is one of the generated dates

        [TestMethod]
        public void Between()
            => DateTimeFilter("Birthdate", "1.1.1995 and 31.12.2000", 546, "between"); // this is one of the generated dates

        [TestMethod]
        public void BetweenNot()
            => DateTimeFilter("Birthdate", "1.1.1995 and 31.12.2000", 10000- 546, "!between"); // this is one of the generated dates

       

        #endregion

        #region DateTime Null filter

        [TestMethod]
        public void NullNotBetween()
            => DateTimeFilter("BirthdateMaybeNull", "1.1.1995 and 31.12.2000", 9727, "!between"); // this is one of the generated dates
        
        [TestMethod]
        public void NullBetween()
            => DateTimeFilter("BirthdateMaybeNull", "1.1.1995 and 31.12.2000", 273, "between"); // this is one of the generated dates

        [TestMethod]
        public void NullBetweenNullAndReal()
            => DateTimeFilter("BirthdateMaybeNull", " and 31.12.2000", 9000, "between"); // this is one of the generated dates

        [TestMethod]
        public void NullEq()
            => DateTimeFilter("BirthdateMaybeNull", "", 10000/2, "=="); // this is one of the generated dates

        [TestMethod]
        public void NullGt()
            => DateTimeFilter("BirthdateMaybeNull", "24.8.1997", 546, ">");// this is one of the generated dates

        [TestMethod]
        public void NullLt()
            => DateTimeFilter("BirthdateMaybeNull", "24.8.1997", 4454, "<");// this is one of the generated dates

        [TestMethod]
        public void NullGtEq()
            => DateTimeFilter("BirthdateMaybeNull", "24.8.1997", 546, ">="); // this is one of the generated dates

        [TestMethod]
        public void NullLtEq()
            => DateTimeFilter("BirthdateMaybeNull", "24.8.1997", 4454, "<="); // this is one of the generated dates

        #endregion


        // test invalid operator
        [TestMethod]
        [ExpectedException(typeof (global::System.Exception))]
        public void NumberFilterInvalidOperator()
            => DateTimeFilter("Birthdate", "180", 5632, "!!");

        public void DateTimeFilter(string attr, string value, int expected, string operation = null)
        {
            var vf = _testDataGeneratedOutsideTimer;
            vf.Attribute = attr;
            vf.Value = value;
            if (operation != null)
                vf.Operator = operation;
            Assert.AreEqual(expected, vf.List.Count(), "Should find exactly " + expected + " amount people");
        }

    }
}
