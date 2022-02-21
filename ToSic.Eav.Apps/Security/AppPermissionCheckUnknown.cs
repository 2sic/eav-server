using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Apps.Security
{
    public sealed class AppPermissionCheckUnknown: AppPermissionCheck, IIsUnknown
    {
        public AppPermissionCheckUnknown(IAppStates appStates, Dependencies dependencies, WarnUseOfUnknown<AppPermissionCheckUnknown> warn) 
            : base(appStates, dependencies) { }
    }
}
