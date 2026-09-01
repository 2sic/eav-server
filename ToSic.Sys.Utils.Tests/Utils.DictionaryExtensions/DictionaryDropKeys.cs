namespace ToSic.Sys.Utils.DictionaryExtensions;

public class DictionaryDropKeys
{
    private IDictionary<string, int> Dictionary => new Dictionary<string, int>
    {
        { "one", 1 },
        { "two", 2 },
        { "three", 3 },
        { "four", 4 },
        { "five", 5 }
    };

    [Fact]
    public void DropKeyTwo()
    {
        var result = Dictionary.FilterOutKeysTac(["two"]);
        Equal(4, result.Count);
        False(result.ContainsKey("two"));
        True(result.ContainsKey("one"));
        True(result.ContainsKey("three"));
    }
    [Fact]
    public void DropKeyTwoAndIrrelevant()
    {
        var result = Dictionary.FilterOutKeysTac(["two", "irrelevant"]);
        Equal(4, result.Count);
        False(result.ContainsKey("two"));
        True(result.ContainsKey("one"));
        True(result.ContainsKey("three"));
    }
    
    
    [Fact]
    public void DropKeysThreeAndFour()
    {
        var result = Dictionary.FilterOutKeysTac(["three", "four"]);
        Equal(3, result.Count);
        False(result.ContainsKey("three"));
        False(result.ContainsKey("four"));
        True(result.ContainsKey("one"));
        True(result.ContainsKey("two"));
        True(result.ContainsKey("five"));
    }

    [Fact]
    public void DropKeysNonExisting()
    {
        var result = Dictionary.FilterOutKeysTac(["six", "seven"]);
        Equal(5, result.Count);
        True(result.ContainsKey("one"));
        True(result.ContainsKey("two"));
        True(result.ContainsKey("three"));
        True(result.ContainsKey("four"));
        True(result.ContainsKey("five"));
    }

    [Fact]
    public void DropAllKeys()
    {
        var result = Dictionary.FilterOutKeysTac(["one", "two", "three", "four", "five"]);
        Empty(result);
    }

    [Fact]
    public void DropNoKeys()
    {
        var result = Dictionary.FilterOutKeysTac([]);
        Equal(5, result.Count);
        True(result.ContainsKey("one"));
        True(result.ContainsKey("two"));
        True(result.ContainsKey("three"));
        True(result.ContainsKey("four"));
        True(result.ContainsKey("five"));
    }

    [Fact]
    public void DropNullKeys()
    {
        Throws<ArgumentNullException>(() => Dictionary.FilterOutKeysTac(null!));
    }
}
