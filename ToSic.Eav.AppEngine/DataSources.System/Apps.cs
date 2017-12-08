using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [VisualQuery(Type = DataSourceType.Source,
        DynamicOut = false,
        Difficulty = DifficultyBeta.Advanced,
        EnableConfig = true,
        ExpectsDataOfType = "fabc849e-b426-42ea-8e1c-c04e69facd9b",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Apps")]

    public sealed class Apps: BaseDataSource
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
	    /// The attribute whoose value will be filtered
	    /// </summary>
	    public int ZoneNumber
	    {
	        get => int.TryParse(Configuration[ZoneKey], out int zid) ? zid : ZoneId;
	        set => Configuration[ZoneKey] = value.ToString();
	    }

	    #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
        public Apps()
		{
            Provide(GetList);
            ConfigMask(ZoneKey, $"[Settings:{ZoneIdField}]");
			//Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
		    //Configuration.Add(ZoneKey, $"[Settings:{ZoneIdField}]");

            //CacheRelevantConfigurations = new [] { ZoneKey };
		}


	    private IEnumerable<IEntity> GetList()
	    {
            EnsureConfigurationIsLoaded();

            // try to load the content-type - if it fails, return empty list
	        var cache = (BaseCache)DataSource.GetCache(ZoneId, AppId);
	        if (!cache.ZoneApps.ContainsKey(ZoneNumber)) return new List<IEntity>();
	        var zone = cache.ZoneApps[ZoneNumber];

	        var list = zone.Apps.OrderBy(a => a.Key).Select(app =>
	        {
	            Eav.Apps.App appObj = null;
	            Guid? guid = null;
	            try
	            {
	                appObj = new Eav.Apps.App(zone.ZoneId, app.Key, false, Log, "for apps DS");
	                if(Guid.TryParse(appObj.AppGuid, out Guid g)) guid = g;
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

	            return AsEntity(appEnt, AppType.Name.ToString(), AppsContentTypeName, app.Key, guid);// new Data.Entity(AppId, app.Key, AppsContentTypeName, appEnt, AppType.Name.ToString(), entityGuid: guid);
            });

            return list;
        }

	}
}