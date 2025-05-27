using System.Text.Json;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Plumbing;

public class IsAnonymousTests
{
    [Fact]
    public void SimpleValues()
    {
        False("".IsAnonymous());
        False(5.IsAnonymous());
        False((null as string).IsAnonymous());
        False(DateTime.Now.IsAnonymous());
        False(Guid.Empty.IsAnonymous());
        False(Array.Empty<string>().IsAnonymous());
    }

    [Fact]
    public void SimpleValueTypes()
    {
        False("".GetType().IsAnonymous());
        False(5.GetType().IsAnonymous());
        //False(((null as string)!).GetType().IsAnonymous());
        False(DateTime.Now.GetType().IsAnonymous());
        False(Guid.Empty.GetType().IsAnonymous());
        False(Array.Empty<string>().GetType().IsAnonymous());
    }

    [Fact]
    public void RealObjects()
    {
        False(new List<string>().IsAnonymous());
        False(new IsAnonymousTests().IsAnonymous());
        False(new Exception().IsAnonymous());
    }

    [Fact]
    public void AnonymousObjects()
    {
        var anon = new
        {
            Key = 27,
            Sub = new
            {
                Key = 42
            },
            Guid = new Guid()
        };
        True(anon.IsAnonymous());
        True(anon.Sub.IsAnonymous());
        False(anon.Guid.IsAnonymous());
    }

    [Fact]
    public void FromJsonAlwaysDictionaries()
    {
        var jsonArray = "[5,3,4]";
        False(JsonSerializer.Deserialize<int[]>(jsonArray).IsAnonymous());

        var jsonObject = "{ \"key\": 27, \"key2\": 42 }";
        False(JsonSerializer.Deserialize<object>(jsonObject).IsAnonymous());

    }
}