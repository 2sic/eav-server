using ToSic.Sys.Context;

namespace ToSic.Eav.Context;

/// <summary>
/// Service to get the current context of a site.
/// </summary>
public interface IContextOfSiteService: IContextService
{
    public new IContextOfSite Context { get; }
}
