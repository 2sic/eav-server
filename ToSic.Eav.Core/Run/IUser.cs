using System;
using System.Collections.Generic;

namespace ToSic.Eav.Run
{
    public interface IUser
    {
        string IdentityToken { get; }

        Guid? Guid { get; }

        List<int> Roles { get; }
    }
}
