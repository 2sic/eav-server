using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repository.EF4;

// important: don't change the namespace just like that, as there are strings referencing it for this loader
// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.SqlSources
{
	/// <summary>
	/// A DataSource that uses SQL Server as Backend
	/// </summary>
	public sealed class EavSqlStore : BaseDataSource, IRootSource
	{
		//private readonly DbDataController _context;
	    private readonly IRepositoryLoader _loader;
		private bool _ready;

  //      #region App/Zone
  //      /// <summary>
		///// Gets the ZoneId of this DataSource
		///// </summary>
		//public override int ZoneId => _context.ZoneId;

	 //   /// <summary>
		///// Gets the AppId of this DataSource
		///// </summary>
		//public override int AppId => _context.AppId;

	 //   #endregion



		/// <summary>
		/// Constructs a new EavSqlStore DataSource
		/// </summary>
		public EavSqlStore()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));
			//_context = DbDataController.Instance();
		    _loader = new Ef4Loader();//_context);

            // 2015-08-08 2dm: note: changed to use the source null (previously this), as it's only used for internal deferred child-entity lookups and would cause infinite looping
            IDictionary<int, IEntity> GetEntities()
                => _loader.CompleteApp(AppId, null, null, true).Entities; // .GetEavEntities(AppId, null);
        }

		/// <summary>
		/// Set Zone and App for this DataSource
		/// </summary>
		public void InitZoneApp(int zoneId, int appId)
		{
			/*_context.*/ZoneId = zoneId;
			/*_context.*/AppId = appId;
			_ready = true;
		}

		public override bool Ready => _ready;

	    public AppDataPackage GetDataForCache(IDeferredEntitiesList targetCacheForDeferredLookups) 
            => _loader.CompleteApp(AppId, null, targetCacheForDeferredLookups);

	    public Dictionary<int, Data.Zone> GetAllZones() => _loader.Zones();// _context.Zone.GetAllZones();

	    public Dictionary<int, string> GetAssignmentObjectTypes() => _loader.MetadataTargetTypes();// _context.DbS.GetAssignmentObjectTypes();
	}
}