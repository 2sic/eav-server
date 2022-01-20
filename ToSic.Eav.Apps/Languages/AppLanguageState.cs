namespace ToSic.Eav.Apps.Languages
{
    public class AppUserLanguageState: SiteLanguageState
    {
        public AppUserLanguageState(string code, string culture, bool isEnabled, bool isAllowed) : base(code, culture, isEnabled)
        {
            IsAllowed = isAllowed;
        }

        public bool IsAllowed { get; }
    }
}
