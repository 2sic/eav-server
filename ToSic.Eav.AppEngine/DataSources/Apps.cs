﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.System;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that gets all Apps of a zone.
    /// </summary>
    [VisualQuery(
        GlobalName = "ToSic.Eav.DataSources.Apps, ToSic.Eav.Apps",
        Type = DataSourceType.Source,
        DynamicOut = false,
        Difficulty = DifficultyBeta.Advanced,
        ExpectsDataOfType = "fabc849e-b426-42ea-8e1c-c04e69facd9b",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps"
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Apps")]
    [PrivateApi("probably should be in own SysInfo folder or something")]
    public sealed class Apps: DataSourceBase
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavAps";

        private const string ZoneKey = "Zone";
        private const string ZoneIdField = "ZoneId";
        private const string AppsContentTypeName = "EAV_Apps";

        // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
	    private const string AppsCtGuid = "11001010-251c-eafe-2792-000000000002";


	    /// <summary>
	    /// The attribute whose value will be filtered
	    /// </summary>
	    public int OfZoneId
	    {
	        get => int.TryParse(Configuration[ZoneKey], out int zid) ? zid : ZoneId;
	        set => Configuration[ZoneKey] = value.ToString();
	    }

	    #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Apps DS
        /// </summary>
        [PrivateApi]
        public Apps()
		{
            Provide(GetList);
            ConfigMask(ZoneKey, $"[Settings:{ZoneIdField}]");
		}


	    private IEnumerable<IEntity> GetList()
	    {
            EnsureConfigurationIsLoaded();

            // try to load the content-type - if it fails, return empty list
	        //var cache = (RootCacheBase)DataSource.GetCache(ZoneId, AppId);
            var cache = Factory.Resolve<IAppsCache>();
	        if (!cache.ZoneApps.ContainsKey(OfZoneId)) return new List<IEntity>();
	        var zone = cache.ZoneApps[OfZoneId];

	        var list = zone.Apps.OrderBy(a => a.Key).Select(app =>
	        {
	            Eav.Apps.App appObj = null;
	            Guid? guid = null;
	            try
	            {
	                appObj = new Eav.Apps.App(zone.ZoneId, app.Key, false, null, Log, "for apps DS");
                    // this will get the guid, if the identity is not "default"
	                if(Guid.TryParse(appObj.AppGuid, out var g)) guid = g;
	            }
	            catch { /* ignore */ }

	            // Assemble the entities
	            var appEnt = new Dictionary<string, object>
	            {
	                {AppType.Id.ToString(), app.Key},
	                {AppType.Name.ToString(), appObj?.Name ?? "error - can't lookup name"},
                    {AppType.Folder.ToString(), appObj?.Folder ?? "" },
                    {AppType.IsHidden.ToString(), appObj?.Hidden ?? false },
	                {AppType.IsDefault.ToString(), app.Key == zone.DefaultAppId},
	            };

	            return AsEntity(appEnt, AppType.Name.ToString(), AppsContentTypeName, app.Key, guid);
            });

            return list;
        }

	}
}