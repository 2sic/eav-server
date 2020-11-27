using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Context
{
    [PrivateApi]
    public interface IUser: IUserLight
    {
        string IdentityToken { get; }

        Guid? Guid { get; }

        List<int> Roles { get; }

        bool IsSuperUser { get; }

        bool IsAdmin { get; }

        /// <summary>
        /// Returns true if a user is in the SexyContent Designers group
        /// </summary>
        bool IsDesigner { get; }

        bool IsAnonymous { get; }
    }
}
