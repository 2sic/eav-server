using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Implementations.UserInformation
{
    public class NeutralEavUser: IUser
    {
        private const string Unknown = "unknown(eav)";

        public string IdentityToken => Unknown;

        public Guid? Guid => null;
    }
}
