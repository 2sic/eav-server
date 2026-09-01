namespace ToSic.Sys.Utils.DictionaryExtensions;

public class DictionaryComparer
{
    [Fact]
    public void GetComparer()
    {
        var comparer = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase).GetComparerTac();
        // The GetComparerTac extension method should delegate to the original GetComparer
        Equal(StringComparer.InvariantCultureIgnoreCase, comparer);
    }
}
