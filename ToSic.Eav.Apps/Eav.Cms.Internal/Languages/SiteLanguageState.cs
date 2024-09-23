﻿namespace ToSic.Eav.Cms.Internal.Languages;

/// <summary>
/// Information about a language in the site - if it's enabled etc.
/// </summary>
/// <param name="code"></param>
/// <param name="culture"></param>
/// <param name="isEnabled"></param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class SiteLanguageState(string code, string culture, bool isEnabled) : ISiteLanguageState
{
    public string Code { get;  } = code;
    public string Culture { get;  } = culture;
    public bool IsEnabled { get;  } = isEnabled;
}