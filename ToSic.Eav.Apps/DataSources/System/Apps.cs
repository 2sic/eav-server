using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that gets all Apps of a zone.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        GlobalName = "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps",
        Type = DataSourceType.Source,
        DynamicOut = false,
        Difficulty = DifficultyBeta.Advanced,
        ExpectsDataOfType = "fabc849e-b426-42ea-8e1c-c04e69facd9b",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps",
                // not sure if this was ever used...just added it for safety for now
                // can probably remove again, if we see that all system queries use the correct name
                "ToSic.Eav.DataSources.Apps, ToSic.Eav.Apps",
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Apps")]
    // ReSharper disable once UnusedMember.Global
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
            // ReSharper disable once UnusedMember.Global
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


	    private List<IEntity> GetList()
	    {
            Configuration.Parse();

            // try to load the content-type - if it fails, return empty list
            //var cache = (RootCacheBase)DataSource.GetCache(ZoneId, AppId);
            var zones = /*Factory.GetAppsCache*/Eav.Apps.State.Zones;
	        if (!zones.ContainsKey(OfZoneId)) return new List<IEntity>();
	        var zone = zones[OfZoneId];

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

                var result = Build.Entity(appEnt,
                    appId: app.Key,
                    id: app.Key,
                    titleField: AppType.Name.ToString(),
                    typeName: AppsContentTypeName, 
                    guid: guid);
                return result;
	            //return AsEntity(appEnt, AppType.Name.ToString(), AppsContentTypeName, app.Key, guid);
            });

            return list.ToList();
        }

	}
}