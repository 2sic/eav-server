namespace ToSic.Eav.Apps.Languages;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class SiteLanguageState: ISiteLanguageState
{
    public SiteLanguageState(string code, string culture, bool isEnabled)
    {
        Code = code;
        Culture = culture;
        IsEnabled = isEnabled;
    }

    public string Code { get;  }
    public string Culture { get;  }
    public bool IsEnabled { get;  }
}