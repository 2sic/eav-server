namespace ToSic.Eav.Apps.Languages
{
    public class AppUserLanguageState: SiteLanguageState
    {
        public AppUserLanguageState(ISiteLanguageState sl, bool isAllowed = true): this(sl.Code, sl.Culture, sl.IsEnabled, isAllowed) 
        {

        }

        public AppUserLanguageState(string code, string culture, bool isEnabled, bool isAllowed) : base(code, culture, isEnabled)
        {
            IsAllowed = isAllowed;
        }

        public bool IsAllowed { get; }
    }
}
