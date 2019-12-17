using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;

namespace ToSic.Eav.Implementations.UserInformation
{
    public class NeutralEavUser: IUser
    {
        private const string Unknown = "unknown(eav)";

        public string IdentityToken => Unknown;

        public Guid? Guid => null;

        public List<int> Roles => new List<int>();
    }
}
