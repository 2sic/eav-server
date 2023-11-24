using System;
using System.Collections.Generic;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Security;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Security
{
    public class EnvironmentPermissionUnknown : EnvironmentPermission
    {
        public EnvironmentPermissionUnknown(WarnUseOfUnknown<EnvironmentPermissionUnknown> _) : base(LogScopes.NotImplemented)
        { }

        public override bool EnvironmentAllows(List<Grants> grants) => UserIsSystemAdmin();

        public override bool VerifyConditionOfEnvironment(string condition) 
            => condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
    }
}
