using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.Interfaces;

//using ToSic.Eav.Repository.EF4;

// important: don't change the namespace just like that, as there are strings referencing it for this loader
// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.SqlSources
{
    /// <inheritdoc cref="BaseDataSource" />
    /// <summary>
    /// A DataSource that uses SQL Server as Backend
    /// </summary>
    public sealed class EavSqlStore : BaseDataSource, IRootSource
	{
        // deferred IRepositoryLoader as needed...
	    public override string LogId => "DS.EavSql";

        private IRepositoryLoader Loader => _ldr ?? (_ldr = Factory.Resolve<IRepositoryLoader>());
	    private IRepositoryLoader _ldr;

	    private bool _ready;


		/// <inheritdoc />
		/// <summary>
		/// Constructs a new EavSqlStore DataSource
		/// </summary>
		public EavSqlStore()
		{
            Log.Rename("EaSqDS");
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, /*GetEntities*/null, GetItems));

            // this function will load data as it's needed
            // note: must use the source null (not "this"), as it's only used for internal deferred child-entity lookups and would cause infinite looping
            //IDictionary<int, IEntity> GetEntities() => Loader.AppPackage(AppId, null, true, Log).Entities;
            IEnumerable<IEntity> GetItems() => Loader.AppPackage(AppId, null, true, Log).Entities.Values;
		}



		/// <summary>
		/// Set Zone and App for this DataSource
		/// </summary>
		public void InitZoneApp(int zoneId, int appId)
		{
			ZoneId = zoneId;
			AppId = appId;
			_ready = true;
		}

		public override bool Ready => _ready;

	    public AppDataPackage GetDataForCache() 
            => Loader.AppPackage(AppId, parentLog: Log);

	    public Dictionary<int, Zone> GetAllZones() => Loader.Zones();

	    //public ImmutableDictionary<int, string> GetAssignmentObjectTypes() => Factory.Resolve<IGlobalMetadataProvider>().TargetTypes; // Loader.MetadataTargetTypes();
	}
}