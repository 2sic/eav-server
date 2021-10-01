using System;
using System.Collections.Generic;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public sealed class AppPermissionCheckUnknown: AppPermissionCheck, IIsUnknown
    {
        public AppPermissionCheckUnknown(IAppStates appStates, Dependencies dependencies, WarnUseOfUnknown<AppPermissionCheckUnknown> warn) 
            : base(appStates, dependencies, LogNames.NotImplemented) { }

        protected override bool EnvironmentAllows(List<Grants> grants) => UserIsSuperuser();

        protected override bool VerifyConditionOfEnvironment(string condition)
        {
            return condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
