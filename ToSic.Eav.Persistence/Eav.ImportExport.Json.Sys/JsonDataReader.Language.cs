using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer
{
    private static IImmutableList<ILanguage> RecreateLanguageList(string languages)
        => languages == NoLanguage
            ? DataConstants.NoLanguages
            : languages.Split(',')
                .Select(ILanguage (a) => new Language(a.Replace(ReadOnlyMarker, ""), a.StartsWith(ReadOnlyMarker)))
                .ToImmutableSafe();


}
