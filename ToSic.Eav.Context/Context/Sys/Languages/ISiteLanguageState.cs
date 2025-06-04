namespace ToSic.Eav.Context.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISiteLanguageState
{
    string Code { get;  }
    string Culture { get;  }
    bool IsEnabled { get;  }
}