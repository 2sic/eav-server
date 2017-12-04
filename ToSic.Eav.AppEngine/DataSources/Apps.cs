using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
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
        private const int DefZone = 0;
        private const string AppsContentTypeName = "EAV_Apps";

        // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
	    private const string AppsCtGuid = "11001010-251c-eafe-2792-000000000002";


        private enum AppsType
	    {
	        Id,
	        Name,
            IsDefault,
            Folder,
            Hidden,
            GlobalId
	    }
	    /// <summary>
	    /// The attribute whoose value will be filtered
	    /// </summary>
	    public int ZoneNumber
	    {
	        get => int.Parse(Configuration[ZoneKey]);
	        set => Configuration[ZoneKey] = value.ToString();
	    }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
        public Apps()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
		    Configuration.Add(ZoneKey, $"[Settings:{ZoneIdField}||{DefZone}]");

            CacheRelevantConfigurations = new [] { ZoneKey };
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
	            try
	            {
	                appObj = new Eav.Apps.App(zone.ZoneId, app.Key, false, Log, "for apps DS");
	            }
	            catch { /* ignore */ }

	            // Assemble the entities
	            var appEnt = new Dictionary<string, object>
	            {
	                {AppsType.Id.ToString(), app.Key},
	                {AppsType.Name.ToString(), appObj?.Name ?? "error - can't lookup name"},
                    {AppsType.Folder.ToString(), appObj?.Folder ?? "" },
                    {AppsType.Hidden.ToString(), appObj?.Hidden ?? false },
	                {AppsType.IsDefault.ToString(), app.Key == zone.DefaultAppId},
                    {AppsType.GlobalId.ToString(), appObj?.AppGuid ?? ""}
	            };

                return new Data.Entity(AppId, app.Key, AppsContentTypeName, appEnt, AppsType.Name.ToString());
            });

            return list;
        }

	}
}