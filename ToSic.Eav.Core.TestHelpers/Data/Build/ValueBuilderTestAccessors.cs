using System.Collections.Immutable;

namespace ToSic.Eav.Data.Build;

public static class ValueBuilderTestAccessors
{
    /// <summary>
    /// Test accessor to reduce use-count of the real code
    /// </summary>
    /// <returns></returns>
    public static IValue BuildTac(this ValueBuilder vBuilder, ValueTypes type, object value, IList<ILanguage> languages)
        => vBuilder.Build(type, value, languages?.ToImmutableList());

    public static IValue BuildTac(this ValueBuilder vBuilder, ValueTypes type, object value, IImmutableList<ILanguage> languages)
        => vBuilder.Build(type, value, languages);
}