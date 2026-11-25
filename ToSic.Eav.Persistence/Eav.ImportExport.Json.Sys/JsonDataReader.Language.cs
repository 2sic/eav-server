using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataReader
{
    private static IImmutableList<ILanguage> RecreateLanguageList(string languages)
        => languages == JsonDataConstants.NoLanguage
            ? DataConstants.NoLanguages
            : languages.Split(',')
                .Select(ILanguage (a) => new Language(a.Replace(JsonDataConstants.ReadOnlyMarker, ""), a.StartsWith(JsonDataConstants.ReadOnlyMarker)))
                .ToImmutableSafe();


}
