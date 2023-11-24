﻿namespace ToSic.Eav.Apps.Languages;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISiteLanguageState
{
    string Code { get;  }
    string Culture { get;  }
    bool IsEnabled { get;  }
}