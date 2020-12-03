using System;
using System.Collections.Generic;
using ToSic.Eav.Context;

namespace ToSic.Eav.Run.Basic
{
    public class BasicUser: IUser
    {
        public string IdentityToken => "basic(eav):0";

        public Guid? Guid => null;

        public List<int> Roles => new List<int>();

        public bool IsSuperUser => false;

        public bool IsAdmin => false;

        public bool IsDesigner => false;

        public int Id => 0;

        public bool IsAnonymous => true;
    }
}
