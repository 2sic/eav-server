﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Context
{
    [PrivateApi]
    public interface IUser
    {

        /// <summary>
        /// User Id as int. Works in DNN and Oqtane
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The identity token is usually something like "dnn:27" or similar.
        /// It's used in two places: for marking changes in the DB (so we know who did it) and as an identifier in the history (so the history can show who made changes).
        /// </summary>
        string IdentityToken { get; }

        /// <summary>
        /// An optional user GUID.
        /// Can be null on systems which don't give the user a unique guid.
        /// </summary>
        Guid? Guid { get; }

        string Username { get; }

        /// <summary>
        /// List of roles the user is in. This is used in permissions check.
        /// Still WIP - probably not the ideal interface, we'll probably change it to something else where the role must not be Int-based. 
        /// </summary>
        [WorkInProgressApi("This may still change, as it's probably better to implement something like IsInRole or something similar.")]
        List<int> Roles { get; }

        /// <summary>
        /// Information if the user has super-user rights. This kind of user can do everything, incl. create apps. 
        /// </summary>
        [Obsolete("deprecated in v14.09 2022-10, will be removed ca. v16 #remove16")]
        bool IsSuperUser { get; }

        /// <summary>
        /// Information if the user has super-user rights. This kind of user can do everything, incl. create apps. 
        /// </summary>
        /// <remarks>
        /// Renamed in v14.09 from IsSuperUser to this to be consistent with other APIs.
        /// </remarks>
        bool IsSystemAdmin { get; }


        /// <summary>
        /// Information if the user is admin - allowing full content-management. 
        /// </summary>
        [Obsolete("deprecated in v14.09 2022-10, will be removed ca. v16 #remove16")]
        bool IsAdmin { get; }


        /// <summary>
        /// Information if the user is admin - allowing full content-management.
        /// </summary>
        /// <remarks>
        /// Renamed in v14.09 from IsAdmin to this to be consistent with other APIs.
        /// </remarks>
        bool IsSiteAdmin { get; }

        /// <summary>
        /// Determines if the user is a content admin. 
        /// </summary>
        /// <remarks>New in v14.09</remarks>
        bool IsContentAdmin { get; }

        /// <summary>
        /// Returns true if a user is in the SexyContent Designers group. Such a person can actually do a lot more, like access the advanced toolbars. 
        /// </summary>
        bool IsDesigner { get; }

        /// <summary>
        /// Info if the user is anonymous / not logged in. 
        /// </summary>
        bool IsAnonymous { get; }
    }
}
