using System;
using System.Collections.Generic;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Context
{
    public sealed class UserUnknown: IUser, IIsUnknown
    {
        public UserUnknown(WarnUseOfUnknown<UserUnknown> warn) { }

        /// <summary>
        /// Simple setting to pretend the user is a super-user in test-scenarios
        /// </summary>
        public static bool AllowEverything = false;

        public string IdentityToken => "unknown(eav):0";

        public Guid? Guid => System.Guid.Empty;

        public List<int> Roles => new List<int>();

        public bool IsSuperUser => AllowEverything;

        public bool IsAdmin => AllowEverything;

        public bool IsDesigner => AllowEverything;

        public int Id => 0;

        public bool IsAnonymous => !AllowEverything;
    }
}
