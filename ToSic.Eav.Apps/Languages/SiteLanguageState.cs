namespace ToSic.Eav.Apps.Languages
{
    public class SiteLanguageState: ISiteLanguageState
    {
        public SiteLanguageState(string code, string text, bool isEnabled)
        {
            Code = code;
            Culture = text;
            IsEnabled = isEnabled;
        }

        public string Code { get;  }
        public string Culture { get;  }
        public bool IsEnabled { get;  }
    }
}