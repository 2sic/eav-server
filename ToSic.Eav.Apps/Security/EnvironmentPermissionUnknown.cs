using System;
using System.Collections.Generic;
using ToSic.Eav.Run.Unknown;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public class EnvironmentPermissionUnknown : EnvironmentPermission
    {
        public EnvironmentPermissionUnknown(WarnUseOfUnknown<EnvironmentPermissionUnknown> warn) : base(LogNames.NotImplemented)
        { }

        public override bool EnvironmentAllows(List<Grants> grants)
        {
            return UserIsSuperuser();
        }

        public override bool VerifyConditionOfEnvironment(string condition)
        {
            return condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
