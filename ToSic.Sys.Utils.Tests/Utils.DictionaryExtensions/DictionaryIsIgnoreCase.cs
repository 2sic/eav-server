using System.Collections.Immutable;
using static System.StringComparer;

namespace ToSic.Sys.Utils.DictionaryExtensions;

public class DictionaryIsIgnoreCase
{
    [Fact]
    public void IsIgnoreCaseDetectsDefaultFalse()
        => False(new Dictionary<string, int>().IsIgnoreCaseTac());

    [Fact]
    public void IsIgnoreCaseDetectsCurrentCultureFalse()
        => False(new Dictionary<string, int>(CurrentCulture).IsIgnoreCaseTac());

    [Fact]
    public void IsIgnoreCaseDetectsOrdinalIgnore()
        => True(new Dictionary<string, int>(OrdinalIgnoreCase).IsIgnoreCaseTac());

    [Fact]
    public void IsIgnoreCaseDetectsInvariantIgnore()
        => True(new Dictionary<string, int>(InvariantCultureIgnoreCase).IsIgnoreCaseTac());

    [Fact]
    public void IsIgnoreCaseDetectsImmutableDefault()
        => True(ImmutableDictionary<string, int>.Empty.IsIgnoreCaseTac());

}