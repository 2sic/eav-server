using ToSic.Eav.Apps;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;

namespace ToSic.Eav.Integration;

/// <summary>
/// Base class for other zone mappers.
/// Has prepared code which should be the same across implementations. 
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class ZoneMapperBase(IAppsCatalog appsCatalog, string logName, object[] connect)
    : ServiceBase(logName, connect: [appsCatalog, ..connect]), IZoneMapper
{
    protected readonly IAppsCatalog AppsCatalog = appsCatalog;

    /// <inheritdoc />
    public abstract int GetZoneId(int siteId);

    /// <inheritdoc />
    public abstract ISite SiteOfZone(int zoneId);

    /// <inheritdoc />
    public ISite SiteOfApp(int appId)
    {
        var l = Log.Fn<ISite>($"{appId}");
        var appIdentifier = AppsCatalog.AppIdentity(appId);
        var site = SiteOfZone(appIdentifier.ZoneId);
        return l.Return(site);
    }

    /// <inheritdoc />
    public abstract List<ISiteLanguageState> CulturesWithState(ISite site);

    public List<ISiteLanguageState> CulturesEnabledWithState(ISite site)
        => CulturesWithState(site).Where(c => c.IsEnabled).ToList();
}