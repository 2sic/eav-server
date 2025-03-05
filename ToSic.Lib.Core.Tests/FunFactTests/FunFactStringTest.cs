using ToSic.Lib.FunFact;
using static Xunit.Assert;

namespace ToSic.Lib.Core.Tests.FunFactTests;


public class FunFactStringTest
{

    [Fact]
    public void ManualListOfFunctions()
    {
        var result = new FunFactString(null,
            [
                ("", _ => "Hello"),
                ("", s => s + " World"),
                ("", s => s + "!")
            ])
            .CreateResult();
        Equal("Hello World!", result);
    }

    [Fact]
    public void SetReplaceOriginal()
    {
        var result = new FunFactString(null,
            [
                ("", _ => "Hello")
            ])
            .Set("World")
            .CreateResult();
        Equal("World", result);
    }

    [Fact]
    public void AppendToOriginal()
    {
        var result = new FunFactString(null, [])
            .Set("Hello")
            .Append(" World")
            .CreateResult();
        Equal("Hello World", result);
    }

    [Fact]
    public void PrependToOriginal()
    {
        var result = new FunFactString(null, [])
            .Set("World")
            .Prepend("Hello ")
            .CreateResult();
        Equal("Hello World", result);
    }

    [Fact]
    public void TrimOriginal()
    {
        var result = new FunFactString(null, [])
            .Set(" Hello ")
            .Trim()
            .CreateResult();
        Equal("Hello", result);
    }

    [Fact]
    public void ReplaceInOriginal()
    {
        var result = new FunFactString(null, [])
            .Set("Hello World")
            .Replace("World", "Universe")
            .CreateResult();
        Equal("Hello Universe", result);
    }

    [Fact]
    public void ReplaceInOriginalMultipleTimes()
    {
        var result = new FunFactString(null, [])
            .Set("Hello World World")
            .Replace("World", "Universe")
            .CreateResult();
        Equal("Hello Universe Universe", result);
    }

    [Fact]
    public void ReplaceInOriginalMultipleTimesWithDifferentValues()
    {
        var result = new FunFactString(null, [])
            .Set("Hello World World")
            .Replace("World", "Universe")
            .Replace("Hello", "Hi")
            .CreateResult();
        Equal("Hi Universe Universe", result);
    }

    [Fact]
    public void VerifyActionsAreNotModifiedOnOriginal()
    {
        var first = new FunFactString(null, []);
        var second = first.Set("Hello");
        var third = second.Append(" World!");

        Empty(first.Actions);
        Single(second.Actions);
        Equal(2, third.Actions.Count);

        Equal("", first.CreateResult());
        Equal("Hello", second.CreateResult());
        Equal("Hello World!", third.CreateResult());
    }
}