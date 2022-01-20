namespace ToSic.Eav.Apps.Languages
{
    public class AppUserLanguageState: SiteLanguageState
    {
        public AppUserLanguageState(string code, string text, bool isEnabled, bool isAllowed) : base(code, text, isEnabled)
        {
            IsAllowed = isAllowed;
        }

        public bool IsAllowed { get; }
    }
}
