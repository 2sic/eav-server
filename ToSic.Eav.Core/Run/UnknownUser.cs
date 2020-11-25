using System;
using System.Collections.Generic;

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
    }
}
