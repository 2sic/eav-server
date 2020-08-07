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
    }
}
