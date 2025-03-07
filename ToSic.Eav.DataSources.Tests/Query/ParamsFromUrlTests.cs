using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Lib.Logging;


namespace ToSic.Eav.DataSourceTests.Query;

[TestClass]
public class ParamsFromUrlTests
{
    [TestMethod]
    public void ManyParams()
    {
        var input = @"something=other
key=result
key2=[token]";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        AreEqual(result.Count, 3, "should find 3 items");
        AreEqual(result["key"], "result", "key=result");
        AreEqual(result["key2"], "[token]", "key=result");
    }

    [TestMethod]
    public void SameKey()
    {
        var input = @"something=other
key=result
something=[token]";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        AreEqual(result.Count, 2, "should find 2 items");
        AreEqual(result["something"], "other", "should be the first set, second should be ignored");
    }
    [TestMethod]
    public void IgnoreComments()
    {
        var input = @"// this is a comment
key=result
key2=[token]";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        AreEqual(result.Count, 2, "should find 2 items");
        AreEqual(result["key"], "result", "key=result");
    }

    [TestMethod]
    public void SingleLine()
    {
        var input = @"something=other";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        AreEqual(result.Count, 1, "should find 1 items");
        AreEqual(result["something"], "other", "should be the first set, second should be ignored");
    }

    [TestMethod]
    public void KeyEqualsOnly()
    {
        var input = @"something=";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        AreEqual(result.Count, 1, "should find 1 items");
        AreEqual(result["something"], "", "should be the first set, second should be ignored");
    }

    [TestMethod]
    public void KeyOnly()
    {
        var input = @"something";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        AreEqual(result.Count, 1, "should find 1 items");
        AreEqual(result["something"], "", "should be the first set, second should be ignored");
    }
}