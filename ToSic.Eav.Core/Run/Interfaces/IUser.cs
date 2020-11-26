using System;
using System.Collections.Generic;

namespace ToSic.Eav.Run
{
    public interface IUser
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

        // new in 11.11?
        int Id { get; }

        bool IsAnonymous {get; }

    }
}
