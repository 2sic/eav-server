using ToSic.Eav.Internal.Unknown;
using ToSic.Lib;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Sys.Users;

internal sealed class UserUnknown(WarnUseOfUnknown<UserUnknown> _) : IUser, IIsUnknown
{
    public string IdentityToken => "unknown(eav):0";

    public Guid Guid => System.Guid.Empty;

    public List<int> Roles => [];

    public bool IsSystemAdmin => false;

    public bool IsSiteAdmin => false;

    public bool IsContentAdmin => false;

    public bool IsContentEditor => false;

    public bool IsSiteDeveloper => false;

    public int Id => 0;

    public string Username => LibConstants.Unknown;

    public string Name => Username;

    public string Email => "unknown@unknown.org";

    public bool IsAnonymous => !false;
}