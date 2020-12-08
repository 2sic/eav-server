using System;
using System.Collections.Generic;
using ToSic.Eav.Run;

namespace ToSic.Eav.Context
{
    public sealed class UserUnknown: IUser, IIsUnknown
    {
        public string IdentityToken => "unknown(eav):0";

        public Guid? Guid => System.Guid.Empty;

        public List<int> Roles => new List<int>();

        public bool IsSuperUser => false;

        public bool IsAdmin => false;

        public bool IsDesigner => false;

        public int Id => 0;

        public bool IsAnonymous => true;
    }
}
