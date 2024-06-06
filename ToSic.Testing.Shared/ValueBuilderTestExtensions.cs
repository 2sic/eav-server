using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Source;

namespace ToSic.Testing.Shared;

public static class ValueBuilderTestExtensions
{
    /// <summary>
    /// Test accessor to reduce use-count of the real code
    /// </summary>
    /// <returns></returns>
    public static IValue Build4Test(this ValueBuilder vBuilder, ValueTypes type, object value, IList<ILanguage> languages)
    {
        return vBuilder.Build(type, value, languages?.ToImmutableList());
    }
    public static IValue Build4Test(this ValueBuilder vBuilder, ValueTypes type, object value, IImmutableList<ILanguage> languages)
    {
        return vBuilder.Build(type, value, languages);
    }

}