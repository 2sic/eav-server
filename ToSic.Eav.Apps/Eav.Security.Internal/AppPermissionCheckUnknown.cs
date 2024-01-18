using ToSic.Eav.Apps;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Security.Internal;

internal sealed class AppPermissionCheckUnknown: AppPermissionCheck, IIsUnknown
{
    public AppPermissionCheckUnknown(IAppStates appStates, MyServices services, WarnUseOfUnknown<AppPermissionCheckUnknown> _) 
        : base(appStates, services) { }
}