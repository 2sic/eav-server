using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Interfaces;

//using ToSic.Eav.Repository.EF4;

// important: don't change the namespace just like that, as there are strings referencing it for this loader
// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.SqlSources
{
	/// <summary>
	/// A DataSource that uses SQL Server as Backend
	/// </summary>
	public sealed class EavSqlStore : BaseDataSource, IRootSource
	{
        // deferred IRepositoryLoader as needed...
	    private IRepositoryLoader Loader => _ldr ?? (_ldr = Factory.Resolve<IRepositoryLoader>());
	    private IRepositoryLoader _ldr;

	    private bool _ready;


		/// <summary>
		/// Constructs a new EavSqlStore DataSource
		/// </summary>
		public EavSqlStore()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntities));

            // this function will load data as it's needed
            // note: must use the source null (not "this"), as it's only used for internal deferred child-entity lookups and would cause infinite looping
            IDictionary<int, IEntity> GetEntities() => Loader.AppPackage(AppId, null, null, true).Entities;
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
            => Loader.AppPackage(AppId, null, targetCacheForDeferredLookups);

	    public Dictionary<int, Data.Zone> GetAllZones() => Loader.Zones();// _context.Zone.GetAllZones();

	    public Dictionary<int, string> GetAssignmentObjectTypes() => Loader.MetadataTargetTypes();// _context.DbS.GetAssignmentObjectTypes();
	}
}