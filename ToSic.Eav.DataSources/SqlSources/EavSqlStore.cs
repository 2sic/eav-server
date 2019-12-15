//using System.Collections.Generic;
//using ToSic.Eav.Apps;
//using ToSic.Eav.Apps.Caching;
//using ToSic.Eav.Documentation;
//using ToSic.Eav.Repositories;
//using AppState = ToSic.Eav.Apps.AppState;
//using IEntity = ToSic.Eav.Data.IEntity;

//// important: don't change the namespace just like that, as there are strings referencing it for this loader
//// ReSharper disable once CheckNamespace
//namespace ToSic.Eav.DataSources.SqlSources
//{
//    /// <summary>
//    /// A DataSource that uses SQL Server as Backend
//    /// </summary>
//    [PrivateApi("not sure if this should be as a data-source, since it's probably never really used as a DS")]
//    public sealed class EavSqlStore : DataSourceBase, IAppsLoader
//	{
//        [PrivateApi]
//	    public override string LogId => "DS.EavSql";

//        private IRepositoryLoader Loader => _ldr ?? (_ldr = Factory.Resolve<IRepositoryLoader>());
//	    private IRepositoryLoader _ldr;

//	    private bool _ready;


//		/// <inheritdoc />
//		/// <summary>
//		/// Constructs a new EavSqlStore DataSource
//		/// </summary>
//		public EavSqlStore()
//		{
//            Log.Rename("EaSqDS");
//			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetItems));

//            // this function will load data as it's needed
//            IEnumerable<IEntity> GetItems() => Loader.AppPackage(AppId, null, Log).List;
//		}

//		/// <inheritdoc />
//		/// <summary>
//		/// Set Zone and App for this DataSource
//		/// </summary>
//		public void InitZoneApp(int zoneId, int appId)
//		{
//			ZoneId = zoneId;
//			AppId = appId;
//			_ready = true;
//		}

//		public override bool Ready => _ready;

//        public AppState GetDataForCache(string primaryLanguage = null)
//        {
//            if (primaryLanguage != null) Loader.PrimaryLanguage = primaryLanguage;
//            return Loader.AppPackage(AppId, parentLog: Log);
//        }

//	    public Dictionary<int, Zone> GetAllZones() => Loader.Zones();
//	}
//}