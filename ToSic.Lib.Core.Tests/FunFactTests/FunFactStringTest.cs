using ToSic.Lib.FunFact;

namespace ToSic.Lib.Core.Tests.FunFactTests;

[TestClass]
public class FunFactStringTest
{

    [TestMethod]
    public void ManualListOfFunctions()
    {
        var result = new FunFactString(null,
            [
                ("", _ => "Hello"),
                ("", s => s + " World"),
                ("", s => s + "!")
            ])
            .CreateResult();
        Assert.AreEqual("Hello World!", result);
    }

    [TestMethod]
    public void SetReplaceOriginal()
    {
        var result = new FunFactString(null,
            [
                ("", _ => "Hello")
            ])
            .Set("World")
            .CreateResult();
        Assert.AreEqual("World", result);
    }

    [TestMethod]
    public void AppendToOriginal()
    {
        var result = new FunFactString(null, [])
            .Set("Hello")
            .Append(" World")
            .CreateResult();
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void PrependToOriginal()
    {
        var result = new FunFactString(null, [])
            .Set("World")
            .Prepend("Hello ")
            .CreateResult();
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void TrimOriginal()
    {
        var result = new FunFactString(null, [])
            .Set(" Hello ")
            .Trim()
            .CreateResult();
        Assert.AreEqual("Hello", result);
    }

    [TestMethod]
    public void ReplaceInOriginal()
    {
        var result = new FunFactString(null, [])
            .Set("Hello World")
            .Replace("World", "Universe")
            .CreateResult();
        Assert.AreEqual("Hello Universe", result);
    }

    [TestMethod]
    public void ReplaceInOriginalMultipleTimes()
    {
        var result = new FunFactString(null, [])
            .Set("Hello World World")
            .Replace("World", "Universe")
            .CreateResult();
        Assert.AreEqual("Hello Universe Universe", result);
    }

    [TestMethod]
    public void ReplaceInOriginalMultipleTimesWithDifferentValues()
    {
        var result = new FunFactString(null, [])
            .Set("Hello World World")
            .Replace("World", "Universe")
            .Replace("Hello", "Hi")
            .CreateResult();
        Assert.AreEqual("Hi Universe Universe", result);
    }

    [TestMethod]
    public void VerifyActionsAreNotModifiedOnOriginal()
    {
        var first = new FunFactString(null, []);
        var second = first.Set("Hello");
        var third = second.Append(" World!");

        Assert.AreEqual(0, first.Actions.Count);
        Assert.AreEqual(1, second.Actions.Count);
        Assert.AreEqual(2, third.Actions.Count);

        Assert.AreEqual("", first.CreateResult());
        Assert.AreEqual("Hello", second.CreateResult());
        Assert.AreEqual("Hello World!", third.CreateResult());
    }
}