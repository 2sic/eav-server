using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Security;

public sealed class AppPermissionCheckUnknown: AppPermissionCheck, IIsUnknown
{
    public AppPermissionCheckUnknown(IAppStates appStates, MyServices services, WarnUseOfUnknown<AppPermissionCheckUnknown> _) 
        : base(appStates, services) { }
}