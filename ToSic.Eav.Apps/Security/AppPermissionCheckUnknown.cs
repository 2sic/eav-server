using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Apps.Security
{
    public sealed class AppPermissionCheckUnknown: AppPermissionCheck, IIsUnknown
    {
        public AppPermissionCheckUnknown(IAppStates appStates, Dependencies dependencies, WarnUseOfUnknown<AppPermissionCheckUnknown> _) 
            : base(appStates, dependencies) { }
    }
}
