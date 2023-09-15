using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests
{
    internal static class TestStringExtensions
    {
        internal static string TestAfter(this string v, string key, bool caseSensitive = false) =>
            v.After(key, caseSensitive);
        //internal static string TestBefore(this string v, string key, bool caseSensitive = false) =>
        //    v.Before(key, caseSensitive);
        internal static string TestBetween(this string value, string before, string after,
            bool goToEndIfEndNotFound = false, bool caseSensitive = false)
            => value.Between(before, after, goToEndIfEndNotFound, caseSensitive);
    }

    [TestClass]
    public class SubStringTests
    {
        [TestMethod]
        public void AfterBasic() => AreEqual("result", "before stuff and something=result".TestAfter("something="));

        [TestMethod]
        public void AfterBasicCaseInsensitive() => AreEqual("result", "before stuff and something=result".TestAfter("SOMETHING="));

        [TestMethod]
        public void AfterBasicCaseSensitive() => AreEqual(null, "before stuff and something=result".TestAfter("SOMETHING=", true));

        [TestMethod]
        public void AfterNotFound() => AreEqual(null, "before stuff and something=result".TestAfter("doesn't exist"));

        [TestMethod]
        public void AfterNotFoundWithDefined() => AreEqual("", "before stuff and something=result".TestAfter("doesn't exist") ?? "");

        [TestMethod]
        public void AfterAtEnd() => AreEqual("", "before stuff and something=result".TestAfter("result"));

        [TestMethod]
        public void AfterWithNullValue() => AreEqual(null, ((string)null).TestAfter("result"));
        
        [TestMethod]
        public void AfterWithNullKeyAndValue() => AreEqual(null, ((string)null).TestAfter(null));

        [TestMethod]
        public void AfterWithNullKey() => AreEqual(null, "something".TestAfter(null));

        [TestMethod]
        public void AfterMultiple() => AreEqual("y x=7 x=99", "before stuff and x=y x=7 x=99".TestAfter("x="));

        // 2023-09-15 2dm removing, as it's now used from RazorBlade...
        //[TestMethod]
        //public void AfterMultipleLast() => AreEqual("99", "before stuff and x=y x=7 x=99".AfterLast("x="));

        // BEFORE

        // 2023-09-15 2dm removing, as it's now used from RazorBlade...
        //[TestMethod] public void BeforeBasic() => AreEqual("before stuff and ", "before stuff and something=result".TestBefore("something="));
        //[TestMethod] public void BeforeEmpty() => AreEqual("", "before stuff and something=result".TestBefore(""));
        //[TestMethod] public void BeforeNull() => AreEqual(null, "before stuff and something=result".TestBefore(null));
        //[TestMethod] public void BeforeOfNull() => AreEqual(null, ((string)null).TestBefore("something"));
        //[TestMethod] public void BeforeNotFound() => AreEqual(null, "before stuff and something=result".TestBefore("doesn't exist"));
        //[TestMethod] public void BeforeMultiple() => AreEqual("before stuff ", "before stuff and and something=result".TestBefore("and"));
        //[TestMethod] public void BeforeMultipleLast() => AreEqual("before stuff and ", "before stuff and and something=result".BeforeLast("and"));

        // BETWEEN
        [TestMethod] public void BetweenBasic() => AreEqual(" stuff ", "before stuff and something=result".TestBetween("before", "and"));
        [TestMethod] public void BetweenNull() => AreEqual(null, ((string)null).TestBetween("before", "and"));
        [TestMethod] public void BetweenStartNotFound() => AreEqual(null, "before stuff and something=result".TestBetween("don't exist", "and"));
        [TestMethod] public void BetweenEndNotFound() => AreEqual(null, "before stuff and something=result".TestBetween("before", "not found"));
        [TestMethod] public void BetweenEndNotFoundToEnd() => AreEqual(" stuff and something=result", "before stuff and something=result".TestBetween("before", "not found", true));

    }
}
