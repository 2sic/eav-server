using ToSic.Sys.Context;

namespace ToSic.Eav.Context;

public interface IContextOfSiteService: IContextService
{
    public new IContextOfSite Context { get; }
}
