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
    public void Alpha() => NotEqual(string.Empty, CreateDicSource().GetTac("Alpha"));

    [Fact]
    public void alpha() => NotEqual(string.Empty, CreateDicSource().GetTac("alpha"));

    [Fact]
    public void Bravo() => NotEqual(string.Empty, CreateDicSource().GetTac("Bravo"));

    [Fact]
    public void Charlie() => Equal(string.Empty, CreateDicSource().GetTac("Charlie"));

    [Fact]
    public void AlphaWithDefault() => Equal("found", CreateDicSource().GetTac("Alpha", ""));

    [Fact]
    public void BravoWithDefault() => Equal("found it too", CreateDicSource().GetTac("Bravo", ""));

    [Fact]
    public void GrandchildWithDefault() => Equal("found", CreateDicSource().GetTac("Child:Grandchild", ""));

    [Fact]
    public void ChildWithDefault() => Equal(string.Empty, CreateDicSource().GetTac("Child", ""));

}