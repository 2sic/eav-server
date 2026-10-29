using ToSic.Sys.Users;

namespace ToSic.Sys.Context.Sys;

internal class ContextOfUserService(IUser user): IContextService
{
    public IContextOfUser Context => new ContextOfUser(user);
}
