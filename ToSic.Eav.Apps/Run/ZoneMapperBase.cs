﻿using System.Collections.Generic;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Run;

/// <summary>
/// Base class for other zone mappers.
/// Has prepared code which should be the same across implementations. 
/// </summary>
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