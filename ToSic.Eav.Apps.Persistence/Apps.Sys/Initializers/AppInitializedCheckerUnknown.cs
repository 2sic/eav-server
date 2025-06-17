#pragma warning disable CS9113 // Parameter is unread.
namespace ToSic.Eav.Apps.Sys.Initializers;
public class AppInitializedCheckerUnknown(WarnUseOfUnknown<AppInitializedCheckerUnknown> _): IAppInitializedChecker
{
    public bool EnsureAppConfiguredAndInformIfRefreshNeeded(IAppReader appIdentity, string? newAppName, CodeRefTrail codeRefTrail, ILog parentLog)
    {
        return false;
    }
}
