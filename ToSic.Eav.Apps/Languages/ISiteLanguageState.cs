namespace ToSic.Eav.Apps.Languages
{
    public interface ISiteLanguageState
    {
        string Code { get;  }
        string Culture { get;  }
        bool IsEnabled { get;  }
    }
}
