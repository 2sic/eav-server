using System.Collections.Immutable;
using ToSic.Sys.Utils;
using static System.StringComparer;

namespace ToSic.Eav.Internal.Plumbing.Generics;

public class DictionaryIsIgnoreCase
{
    private static bool IsIgnoreCaseTac<T>(IDictionary<string, T> dic) => dic.IsIgnoreCase();

    [Fact]
    public void IsIgnoreCaseDetectsDefaultFalse()
        => False(IsIgnoreCaseTac(new Dictionary<string, int>()));

    [Fact]
    public void IsIgnoreCaseDetectsCurrentCultureFalse()
        => False(IsIgnoreCaseTac(new Dictionary<string, int>(CurrentCulture)));

    [Fact]
    public void IsIgnoreCaseDetectsOrdinalIgnore()
        => True(IsIgnoreCaseTac(new Dictionary<string, int>(OrdinalIgnoreCase)));

    [Fact]
    public void IsIgnoreCaseDetectsInvariantIgnore()
        => True(IsIgnoreCaseTac(new Dictionary<string, int>(InvariantCultureIgnoreCase)));

    [Fact]
    public void IsIgnoreCaseDetectsImmutableDefault()
        => True(IsIgnoreCaseTac(new Dictionary<string, int>().ToImmutableDictionary(InvariantCultureIgnoreCase)));

}