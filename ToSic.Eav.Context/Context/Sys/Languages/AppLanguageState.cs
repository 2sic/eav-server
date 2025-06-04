namespace ToSic.Eav.Context.Sys;

/// <summary>
/// Language information specific to one language in this App for this User.
/// For example, if the user is allowed to edit content in this language.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppUserLanguageState: SiteLanguageState
{
    public AppUserLanguageState(ISiteLanguageState sl, bool isAllowed, int permissionCount) 
        : this(sl.Code, sl.Culture, sl.IsEnabled, isAllowed, permissionCount) 
    { }

    private AppUserLanguageState(string code, string culture, bool isEnabled, bool isAllowed, int permissionCount) : base(code, culture, isEnabled)
    {
        IsAllowed = isAllowed;
        PermissionCount = permissionCount;
    }

    public bool IsAllowed { get; }

    public int PermissionCount { get; }
}