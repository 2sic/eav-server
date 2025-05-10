namespace ToSic.Eav.Cms.Internal.Languages;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISiteLanguageState
{
    string Code { get;  }
    string Culture { get;  }
    bool IsEnabled { get;  }
}