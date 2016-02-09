using System.Collections.Generic;
using ToSic.Eav.BLL;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.DataSources.SqlSources
{
	/// <summary>
	/// A DataSource that uses SQL Server as Backend
	/// </summary>
	public class EavSqlStore : BaseDataSource, IRootSource
	{
		private readonly EavDataController Context;
	    // private readonly DbShortcuts DbS;
		private bool _ready;

        #region App/Zone
        /// <summary>
		/// Gets the ZoneId of this DataSource
		/// </summary>
		public override int ZoneId
		{
			get { return Context.ZoneId; }
		}
		/// <summary>
		/// Gets the AppId of this DataSource
		/// </summary>
		public override int AppId
		{
			get { return Context.AppId; }
		}
        #endregion

        private IDictionary<int, IEntity> GetEntities()
		{
            // 2015-08-08 2dm: note: changed to use the source null (previously this), as it's only used for internal deferred child-entity lookups and would cause infinite looping
			return new DbLoadIntoEavDataStructure(Context).GetEavEntities(AppId, null);
		}

		/// <summary>
		/// Constructs a new EavSqlStore DataSource
		/// </summary>
		public EavSqlStore()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
			Context = EavDataController.Instance();
            // DbS = new DbShortcuts(_context);
		}

		/// <summary>
		/// Set Zone and App for this DataSource
		/// </summary>
		public void InitZoneApp(int zoneId, int appId)
		{
			Context.ZoneId = zoneId;
			Context.AppId = appId;

			_ready = true;
		}

		public override bool Ready { get { return _ready; } }

		public AppDataPackage GetDataForCache(IDeferredEntitiesList targetCacheForDeferredLookups)
		{
			return new DbLoadIntoEavDataStructure(Context).GetAppDataPackage(null, AppId, targetCacheForDeferredLookups);
		}

	    public Dictionary<int, Data.Zone> GetAllZones()
		{
			return Context.Zone.GetAllZones();
		}

		public Dictionary<int, string> GetAssignmentObjectTypes()
		{
			return Context.DbS.GetAssignmentObjectTypes();
		}
	}
}