using ToSic.Sys.Context;
using ToSic.Sys.Users;

namespace ToSic.Eav.Context;

internal class ContextOfSiteService(ISite site, IUser user) : IContextOfSiteService
{
    IContextOfUser IContextService.Context => Context;

    public IContextOfSite Context => field ??= new ContextOfSite(site, user);
}
