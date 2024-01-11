namespace ToSic.Eav.Cms.Internal.Languages;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class SiteLanguageState(string code, string culture, bool isEnabled) : ISiteLanguageState
{
    public string Code { get;  } = code;
    public string Culture { get;  } = culture;
    public bool IsEnabled { get;  } = isEnabled;
}