using System;
using System.Collections.Generic;

namespace ToSic.Eav.Run.Basic
{
    public class BasicUser: IUser
    {
        private const string Unknown = "unknown(eav)";

        public string IdentityToken => Unknown;

        public Guid? Guid => null;

        public List<int> Roles => new List<int>();

        public bool IsSuperUser => false;

        public bool IsAdmin => false;

        public bool IsDesigner => false;

        public int Id => 0;

        public bool IsAnonymous => true;
    }
}
