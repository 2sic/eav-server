using System;
using System.Collections.Generic;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public class BasicAppPermissionCheck: AppPermissionCheck
    {
        public BasicAppPermissionCheck() : base(LogNames.NotImplemented) { }

        protected override bool EnvironmentAllows(List<Grants> grants) => false;

        protected override bool VerifyConditionOfEnvironment(string condition)
        {
            return condition.Equals("SecurityAccessLevel.Anonymous", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
