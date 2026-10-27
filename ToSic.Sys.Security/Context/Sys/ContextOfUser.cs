using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Sys.Context.Sys;

public record ContextOfUser(IUser User) : IContextOfUser
{
    public EffectivePermissions Permissions
    {
        get => field ??= User.EffectivePermissions();
        init;
    }
}
