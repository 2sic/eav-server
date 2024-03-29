﻿using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Context;

/// <summary>
/// Very basic implementation.
/// Should be replaced in Eav.Apps which often knows more about the user than just the simple site context
/// </summary>
internal class ContextOfUserPermissions(IUser user) : ServiceBase("Eav.CtxSec"), IContextOfUserPermissions
{
    public IUser User { get; } = user;

    public bool UserMayEdit => _userMayEditWip.Get(() => Log.GetterM(() =>
    {
        var u = User;
        if (u == null) return (false, "user unknown, false");
        // Case 1: Superuser always may
        if (u.IsSystemAdmin) return (true, "super");

        return (u.IsSiteAdmin || u.IsSiteDeveloper, "admin/developer");
    }));
    private readonly GetOnce<bool> _userMayEditWip = new();
}