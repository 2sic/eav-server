using System;
using System.Collections.Generic;
using ToSic.Eav.Run.Unknown;
using ToSic.Eav.Security;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Security
{
    public class EnvironmentPermissionUnknown : EnvironmentPermission
    {
        public EnvironmentPermissionUnknown(WarnUseOfUnknown<EnvironmentPermissionUnknown> warn) : base(LogScopes.NotImplemented)
        { }

        public override bool EnvironmentAllows(List<Grants> grants) => UserIsSystemAdmin();

        public override bool VerifyConditionOfEnvironment(string condition) 
            => condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
    }
}
