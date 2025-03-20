using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSource.Query;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class ParamsFromUrlTests
{
    [Fact]
    public void ManyParams()
    {
        var input = @"something=other
key=result
key2=[token]";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        Equal(result.Count, 3);//, "should find 3 items");
        Equal(result["key"], "result");//, "key=result");
        Equal(result["key2"], "[token]");//, "key=result");
    }

    [Fact]
    public void SameKey()
    {
        var input = @"something=other
key=result
something=[token]";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        Equal(result.Count, 2);//, "should find 2 items");
        Equal(result["something"], "other");//, "should be the first set, second should be ignored");
    }
    [Fact]
    public void IgnoreComments()
    {
        var input = @"// this is a comment
key=result
key2=[token]";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        Equal(result.Count, 2);//, "should find 2 items");
        Equal(result["key"], "result");//, "key=result");
    }

    [Fact]
    public void SingleLine()
    {
        var input = @"something=other";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        Equal(result.Count, 1);//, "should find 1 items");
        Equal(result["something"], "other");//, "should be the first set, second should be ignored");
    }

    [Fact]
    public void KeyEqualsOnly()
    {
        var input = @"something=";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        Equal(result.Count, 1);//, "should find 1 items");
        Equal(result["something"], "");//, "should be the first set, second should be ignored");
    }

    [Fact]
    public void KeyOnly()
    {
        var input = @"something";
        var result = QueryDefinition.GenerateParamsDic(input, new Log("dummy"));
        Equal(result.Count, 1);//, "should find 1 items");
        Equal(result["something"], "");//, "should be the first set, second should be ignored");
    }
}