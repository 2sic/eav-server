using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Sys.Context;

public interface IContextOfUser
{
    /// <summary>
    /// The user in the current request / context
    /// </summary>
    public IUser User { get; }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    EffectivePermissions Permissions { get; }
}
