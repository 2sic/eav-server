using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context;

public record ContextOfSite(ISite Site, IUser User) : IContextOfSite
{
    public IContextOfSite WithSwappedSite(ISite site) => this with { Site = site };

    public EffectivePermissions Permissions
    {
        get => field ??= User.EffectivePermissions();
        init;
    }
}
