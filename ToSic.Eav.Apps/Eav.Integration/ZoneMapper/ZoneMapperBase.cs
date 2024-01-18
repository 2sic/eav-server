using ToSic.Eav.Apps;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;

namespace ToSic.Eav.Integration;

/// <summary>
/// Base class for other zone mappers.
/// Has prepared code which should be the same across implementations. 
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class ZoneMapperBase: ServiceBase, IZoneMapper
{

    /// <summary>
    /// Trivial constructor
    /// </summary>
    protected ZoneMapperBase(IAppStates appStates, string logName) : base(logName)
    {
        ConnectServices(
            AppStates = appStates
        );
    }
    protected readonly IAppStates AppStates;

    /// <inheritdoc />
    public abstract int GetZoneId(int siteId);

    /// <inheritdoc />
    public abstract ISite SiteOfZone(int zoneId);

    /// <inheritdoc />
    public ISite SiteOfApp(int appId) => Log.Func($"{appId}", () =>
    {
        var appIdentifier = AppStates.IdentityOfApp(appId);
        var tenant = SiteOfZone(appIdentifier.ZoneId);
        return tenant;
    });

    /// <inheritdoc />
    public abstract List<ISiteLanguageState> CulturesWithState(ISite site);
}