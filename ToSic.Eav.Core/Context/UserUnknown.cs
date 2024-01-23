using System;
using System.Collections.Generic;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Context;

internal sealed class UserUnknown: IUser, IIsUnknown
{
    public UserUnknown(WarnUseOfUnknown<UserUnknown> _) { }

    public string IdentityToken => "unknown(eav):0";

    public Guid Guid => System.Guid.Empty;

    public List<int> Roles => [];

    public bool IsSystemAdmin => false;

    [Obsolete("deprecated in v14.09 2022-10, will be removed ca. v16 #remove16")]
    public bool IsSuperUser => false;

    [Obsolete("deprecated in v14.09 2022-10, will be removed ca. v16 #remove16")]
    public bool IsAdmin => false;

    public bool IsSiteAdmin => false;

    public bool IsContentAdmin => false;

    public bool IsSiteDeveloper => false;

    public int Id => 0;

    public string Username => Constants.NullNameId;

    public string Name => Username;

    public string Email => "unknown@unknown.org";

    public bool IsAnonymous => !false;
}