using static Xunit.Assert;

namespace ToSic.Eav.LookUp;

public class SourceDictionaryTest
{
    private static LookUpInDictionary CreateDicSource()
    {
        var sv = new LookUpInDictionary("Demo");
        sv.Properties.Add("Alpha", "found");
        sv.Properties.Add("Bravo", "found it too");
        sv.Properties.Add("Child:Grandchild", "found");
        return sv;
    }

    [Fact]
    public void Alpha() => NotEqual(string.Empty, CreateDicSource().Get("Alpha"));

    [Fact]
    public void alpha() => NotEqual(string.Empty, CreateDicSource().Get("alpha"));

    [Fact]
    public void Bravo() => NotEqual(string.Empty, CreateDicSource().Get("Bravo"));

    [Fact]
    public void Charlie() => Equal(string.Empty, CreateDicSource().Get("Charlie"));

    [Fact]
    public void AlphaWithDefault() => Equal("found", CreateDicSource().Get("Alpha", ""));

    [Fact]
    public void BravoWithDefault() => Equal("found it too", CreateDicSource().Get("Bravo", ""));

    [Fact]
    public void GrandchildWithDefault() => Equal("found", CreateDicSource().Get("Child:Grandchild", ""));

    [Fact]
    public void ChildWithDefault() => Equal(string.Empty, CreateDicSource().Get("Child", ""));

}