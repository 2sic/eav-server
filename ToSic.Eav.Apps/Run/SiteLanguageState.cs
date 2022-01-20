// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    public class SiteLanguageState: ISiteLanguageState
    {
        public SiteLanguageState(string code, string text, bool active)
        {
            Code = code;
            Culture = text;
            IsEnabled = active;
        }

        public string Code { get;  }
        public string Culture { get;  }
        public bool IsEnabled { get;  }
    }
}