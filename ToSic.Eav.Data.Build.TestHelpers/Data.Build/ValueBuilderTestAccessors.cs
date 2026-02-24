using System.Collections.Immutable;

namespace ToSic.Eav.Data.Build;

public static class ValueBuilderTestAccessors
{
    /// <summary>
    /// Test accessor to reduce use-count of the real code
    /// </summary>
    /// <returns></returns>
    public static IValue BuildTac(this ValueAssembler vAssembler, ValueTypes type, object value, IList<ILanguage> languages)
        => vAssembler.Create(type, value, languages?.ToImmutableOpt());

    public static IValue BuildTac(this ValueAssembler vAssembler, ValueTypes type, object value, IImmutableList<ILanguage> languages)
        => vAssembler.Create(type, value, languages);
}