﻿using ToSic.Eav.Context;
using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Eav.Integration;
using ToSic.Eav.Services;

// NOTE 2023-01-11 refactoring - was previously ToSic.Eav.Apps.App - renamed to ToSic.Eav.Apps.Internal.EavApp
// Could be a breaking change

namespace ToSic.Eav.Apps.Internal;

/// <summary>
/// A <em>single-use</em> app-object providing quick simple api to access
/// name, folder, data, metadata etc.
/// </summary>
/// <param name="services">All the dependencies of this app, managed by this app</param>
/// <param name="logName">must be null by default, because of DI</param>
[PrivateApi("Hide implementation - was PublicApi_Stable_ForUseInYourCode till 16.09")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract partial class EavApp(EavApp.MyServices services, string logName = default, object[] connect = default) : AppBase<EavApp.MyServices>(services, logName ?? "Eav.App", connect: connect), IApp
{
    #region Constructor / DI

    /// <summary>
    /// Helper class, so inheriting stuff doesn't need to update the constructor all the time
    /// </summary>
    [PrivateApi]
    public class MyServices(
        IZoneMapper zoneMapper,
        ISite site,
        IAppStates appStates,
        IDataSourcesService dataSourceFactory,
        LazySvc<QueryManager> queryManager,
        IAppDataConfigProvider dataConfigProvider)
        : MyServicesBase(connect: [zoneMapper, site, appStates, dataSourceFactory, queryManager, dataConfigProvider])
    {
        public IAppDataConfigProvider DataConfigProvider { get; } = dataConfigProvider;
        public LazySvc<QueryManager> QueryManager { get; } = queryManager;
        internal IZoneMapper ZoneMapper { get; } = zoneMapper;
        internal ISite Site { get; } = site;
        internal IAppStates AppStates { get; } = appStates;
        internal IDataSourcesService DataSourceFactory { get; } = dataSourceFactory;
    }

    #endregion

    /// <inheritdoc />
    public string Name { get; private set; }
    /// <inheritdoc />
    public string Folder { get; private set; }

    // 2024-01-11 2dm - #RemoveIApp.Hidden for v17 - kill code ca. 2024-07 (Q3)
    ///// <inheritdoc />
    //public bool Hidden { get; private set; }

    /// <inheritdoc />
    public string NameId { get; private set; }

    /// <inheritdoc />
    [Obsolete]
    [PrivateApi]
    public string AppGuid => NameId;

    public EavApp Init(ISite site, IAppIdentityPure appIdentity, AppDataConfigSpecs dataSpecs)
    {
        var l = Log.Fn<EavApp>();

        // 2024-03-18 moved here...
        if (site != null) Site = site;

        // Env / Tenant must be re-checked here
        if (Site == null) throw new("no site/portal received");
            
        // in case the DI gave a bad tenant, try to look up
        if (Site.Id == Constants.NullId && appIdentity.AppId != Constants.NullId &&
            appIdentity.AppId != AppConstants.AppIdNotFound)
            Site = Services.ZoneMapper.SiteOfApp(appIdentity.AppId);

        // 2023-12-12 2dm removed autolookup Zone - must happen before this #RemoveAutoLookupZone
        // leave in till 2024-Q2 and remove if all works
        //// if zone is missing, try to find it - but always assume current context
        //if (appIdentity.ZoneId == AppConstants.AutoLookupZone)
        //    appIdentity = new AppIdentityPure(Site.ZoneId, appIdentity.AppId);
            
        InitAppBaseIds(appIdentity);
        l.A($"prep App #{appIdentity.Show()}, has{nameof(dataSpecs)}:{dataSpecs != null}");

        // Look up name in cache
        NameId = Services.AppStates.GetReader(this).NameId;

        InitializeResourcesSettingsAndMetadata();

        // for deferred initialization as needed
        _dataConfigSpecs = dataSpecs;

        return l.Return(this);
    }
}