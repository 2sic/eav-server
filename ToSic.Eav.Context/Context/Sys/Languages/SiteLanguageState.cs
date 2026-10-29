using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Context.Sys;

/// <summary>
/// Information about a language in the site - if it's enabled etc.
/// </summary>
/// <param name="Code"></param>
/// <param name="Culture"></param>
/// <param name="IsEnabled"></param>
[ContentType(
    Name = nameof(SiteLanguageState),
    Guid = "396039ec-0fd4-4b2a-9d91-d67bcbb01686",
    Description = "Language state for a site",
    Scope = "System"
)]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record SiteLanguageState(string Code, string Culture, bool IsEnabled) : ISiteLanguageState, IRawEntityAutoConvert;