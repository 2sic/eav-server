using System;
using System.Collections.Generic;
using ToSic.Eav.Context;

namespace ToSic.Eav.Run
{
    public class UnknownUser: IUser
    {
        public string IdentityToken => "not-a-user:0";
        public Guid? Guid => System.Guid.Empty;
        public List<int> Roles => new List<int>();
        public bool IsSuperUser => false;
        public bool IsAdmin => false;
        public bool IsDesigner => false;

        public int Id => 0;
        public bool IsAnonymous => true;
    }
}
