using System;

namespace ToSic.Eav.Interfaces
{
    public interface IUser
    {
        string IdentityToken { get; }

        Guid? Guid { get; }
    }
}
