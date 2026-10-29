using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Context.Sys;

/// <summary>
/// Language information specific to one language in this App for this User.
/// For example, if the user is allowed to edit content in this language.
/// </summary>
[ContentType(
    Name = nameof(AppLanguageState),
    Guid = "be0ff332-e467-4ca4-9b23-441b06fa8501", // warning: @2rb - you reused this guid in the data source and the raw model "c8676078-b904-4412-bf4e-aa83d48b63e7",
    Description = "Language activation state for an app",
    Scope = "System"
)]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record AppLanguageState(string Code, string Culture, bool IsEnabled, bool IsAllowed, int PermissionCount)
    : SiteLanguageState(Code, Culture, IsEnabled), IRawEntityAutoConvert
{
    public AppLanguageState(ISiteLanguageState sl, bool isAllowed, int permissionCount)
        : this(sl.Code, sl.Culture, sl.IsEnabled, isAllowed, permissionCount)
    { }
}