using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging.Simple;

namespace ToSic.Eav.DataSourceTests.Query
{
    [TestClass]
    public class ParamsTests
    {
        [TestMethod]
        public void ManyParams()
        {
            var input = @"something=other
key=result
key2=[token]";
            var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
            Assert.AreEqual(result.Count, 3, "should find 3 items");
            Assert.AreEqual(result["key"], "result", "key=result");
            Assert.AreEqual(result["key2"], "[token]", "key=result");
        }

        [TestMethod]
        public void SameKey()
        {
            var input = @"something=other
key=result
something=[token]";
            var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
            Assert.AreEqual(result.Count, 2, "should find 2 items");
            Assert.AreEqual(result["something"], "other", "should be the first set, second should be ignored");
        }
        [TestMethod]
        public void IgnoreComments()
        {
            var input = @"// this is a comment
key=result
key2=[token]";
            var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
            Assert.AreEqual(result.Count, 2, "should find 2 items");
            Assert.AreEqual(result["key"], "result", "key=result");
        }

        [TestMethod]
        public void SingleLine()
        {
            var input = @"something=other";
            var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
            Assert.AreEqual(result.Count, 1, "should find 1 items");
            Assert.AreEqual(result["something"], "other", "should be the first set, second should be ignored");
        }

        [TestMethod]
        public void KeyEqualsOnly()
        {
            var input = @"something=";
            var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
            Assert.AreEqual(result.Count, 1, "should find 1 items");
            Assert.AreEqual(result["something"], "", "should be the first set, second should be ignored");
        }

        [TestMethod]
        public void KeyOnly()
        {
            var input = @"something";
            var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
            Assert.AreEqual(result.Count, 1, "should find 1 items");
            Assert.AreEqual(result["something"], "", "should be the first set, second should be ignored");
        }
   }
}
