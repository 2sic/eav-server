using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Plumbing;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests
{
    [TestClass]
    public class SubStringTests
    {
        [TestMethod]
        public void AfterBasic() => AreEqual("result", "before stuff and something=result".After("something="));

        [TestMethod]
        public void AfterBasicCaseInsensitive() => AreEqual("result", "before stuff and something=result".After("SOMETHING="));

        [TestMethod]
        public void AfterBasicCaseSensitive() => AreEqual(null, "before stuff and something=result".After("SOMETHING=", true));

        [TestMethod]
        public void AfterNotFound() => AreEqual(null, "before stuff and something=result".After("doesn't exist"));

        [TestMethod]
        public void AfterNotFoundWithDefined() => AreEqual("", "before stuff and something=result".After("doesn't exist") ?? "");

        [TestMethod]
        public void AfterAtEnd() => AreEqual("", "before stuff and something=result".After("result"));

        [TestMethod]
        public void AfterWithNullValue() => AreEqual(null, ((string)null).After("result"));
        
        [TestMethod]
        public void AfterWithNullKeyAndValue() => AreEqual(null, ((string)null).After(null));

        [TestMethod]
        public void AfterWithNullKey() => AreEqual(null, "something".After(null));

        [TestMethod]
        public void AfterMultiple() => AreEqual("y x=7 x=99", "before stuff and x=y x=7 x=99".After("x="));

        [TestMethod]
        public void AfterMultipleLast() => AreEqual("99", "before stuff and x=y x=7 x=99".AfterLast("x="));

        // BEFORE

        [TestMethod] public void BeforeBasic() => AreEqual("before stuff and ", "before stuff and something=result".Before("something="));
        [TestMethod] public void BeforeEmpty() => AreEqual("", "before stuff and something=result".Before(""));
        [TestMethod] public void BeforeNull() => AreEqual(null, "before stuff and something=result".Before(null));
        [TestMethod] public void BeforeOfNull() => AreEqual(null, ((string)null).Before("something"));
        [TestMethod] public void BeforeNotFound() => AreEqual(null, "before stuff and something=result".Before("doesn't exist"));
        [TestMethod] public void BeforeMultiple() => AreEqual("before stuff ", "before stuff and and something=result".Before("and"));
        [TestMethod] public void BeforeMultipleLast() => AreEqual("before stuff and ", "before stuff and and something=result".BeforeLast("and"));

        // BETWEEN
        [TestMethod] public void BetweenBasic() => AreEqual(" stuff ", "before stuff and something=result".Between("before", "and"));
        [TestMethod] public void BetweenNull() => AreEqual(null, ((string)null).Between("before", "and"));
        [TestMethod] public void BetweenStartNotFound() => AreEqual(null, "before stuff and something=result".Between("don't exist", "and"));
        [TestMethod] public void BetweenEndNotFound() => AreEqual(null, "before stuff and something=result".Between("before", "not found"));
        [TestMethod] public void BetweenEndNotFoundToEnd() => AreEqual(" stuff and something=result", "before stuff and something=result".Between("before", "not found", true));

    }
}
