﻿namespace ToSic.Eav.Cms.Internal.Languages;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppUserLanguageState: SiteLanguageState
{
    public AppUserLanguageState(ISiteLanguageState sl, bool isAllowed, int permissionCount) 
        : this(sl.Code, sl.Culture, sl.IsEnabled, isAllowed, permissionCount) 
    {
    }

    private AppUserLanguageState(string code, string culture, bool isEnabled, bool isAllowed, int permissionCount) : base(code, culture, isEnabled)
    {
        IsAllowed = isAllowed;
        PermissionCount = permissionCount;
    }

    public bool IsAllowed { get; }

    public int PermissionCount { get; }

}