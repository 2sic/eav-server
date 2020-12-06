using System;
using System.Collections.Generic;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public sealed class AppPermissionCheckUnknown: AppPermissionCheck
    {
        public AppPermissionCheckUnknown() : base(LogNames.NotImplemented) { }

        protected override bool EnvironmentAllows(List<Grants> grants) => UserIsSuperuser();

        protected override bool VerifyConditionOfEnvironment(string condition)
        {
            return condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
