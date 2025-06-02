using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Apps.Sys.Initializers;
public class AppInitializedCheckerUnknown(WarnUseOfUnknown<AppInitializedCheckerUnknown> _): IAppInitializedChecker
{
    public bool EnsureAppConfiguredAndInformIfRefreshNeeded(IAppReader appIdentity, string appName, CodeRefTrail codeRefTrail,
        ILog parentLog)
    {
        return false;
    }
}
