using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav
{
	/// <summary>
	/// System to prepare data sources according to our needs.
	/// </summary>
	public class DataSource
	{
	    private const string LogKey = "DS.Factry";
        private static readonly string RootDataSource = typeof(IAppRoot).AssemblyQualifiedName;

        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="sourceName">Full Qualified Type/Interface Name</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="configLookUp">Provides configuration values if needed</param>
        /// <param name="parentLog"></param>
        /// <returns>A single DataSource</returns>
        public static IDataSource GetDataSource(string sourceName, IAppIdentity app, IDataSource upstream = null, ILookUpEngine configLookUp = null, ILog parentLog = null)
		{
            var wrapLog = parentLog?
                .AddChild(LogKey)
                .Call(nameof(GetDataSource), $"with name {sourceName}");
		    // try to find with assembly name, or otherwise with GlobalName / previous names
            var type = Type.GetType(sourceName) 
                ?? Catalog.FindInDsTypeCache(sourceName)?.Type;

		    // still not found? must show error
			if (type == null)
			    throw new Exception("DataSource not installed on Server: " + sourceName);
            var result = GetDataSource(type, app, upstream, configLookUp, parentLog);
            wrapLog?.Invoke("ok");
            return result;
        }


        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="configLookUp">Provides configuration values if needed</param>
        /// <param name="parentLog"></param>
        /// <returns>A single DataSource</returns>
        private static IDataSource GetDataSource(
            Type type, 
            IAppIdentity app, 
            IDataSource upstream,
	        ILookUpEngine configLookUp, ILog parentLog)
        {
            parentLog = FindBestLog(parentLog, app, upstream);
            var wrapLog = parentLog?
                .AddChild(LogKey)
                .Call(nameof(GetDataSource));
	        var newDs = (DataSourceBase) Factory.Resolve(type);
            ConfigureNewDataSource(newDs, app, upstream, configLookUp, parentLog);
            wrapLog?.Invoke("ok");
            return newDs;
	    }

        public static T GetDataSource<T>(
            IDataSource upstream, 
            ILog parentLog = null) where T : IDataSource 
            => GetDataSource<T>(upstream, upstream, upstream.Configuration.LookUps, parentLog);


        public static T GetDataSource<T>(
            IAppIdentity appIdentity, 
            IDataSource upstream, 
            ILookUpEngine configLookUp = null,
            ILog parentLog = null) where T : IDataSource
        {
            parentLog = FindBestLog(parentLog, appIdentity, upstream);
            var wrapLog = parentLog?
                .AddChild(LogKey)
                .Call(nameof(GetDataSource)+ $"<{typeof(T).Name}>");

            if (upstream == null && configLookUp == null)
                throw new Exception(
                    "Trying to GetDataSource<T> but cannot do so if both upstream and ConfigurationProvider are null.");

            var newDs = (DataSourceBase) Factory.Resolve(typeof(T));
            ConfigureNewDataSource(newDs, appIdentity, upstream, configLookUp ?? upstream.Configuration.LookUps, parentLog);
            wrapLog?.Invoke("ok");
            return (T) Convert.ChangeType(newDs, typeof(T));
        }

        /// <summary>
        /// handle missing parent log if we have an upstream
        /// - try to get it from the app-identity, then from upstream
        /// </summary>
        /// <param name="parentLog"></param>
        /// <param name="app"></param>
        /// <param name="upstream"></param>
        /// <returns></returns>
        private static ILog FindBestLog(ILog parentLog, IAppIdentity app, IHasLog upstream) 
            => parentLog ?? (app as IHasLog)?.Log ?? upstream?.Log;

        /// <summary>
        /// Helper function (internal) to configure a new data source. This code is used multiple times, that's why it's in an own function
        /// </summary>
        /// <param name="newDs">The new data source</param>
        /// <param name="appIdentity">app identifier</param>
        /// <param name="upstream">upstream data source - for auto-attaching</param>
        /// <param name="configLookUp">optional configuration provider - for auto-attaching</param>
        /// <param name="parentLog"></param>
        private static void ConfigureNewDataSource(
            DataSourceBase newDs, 
            IAppIdentity appIdentity,
			IDataSource upstream = null,
			ILookUpEngine configLookUp = null, 
            ILog parentLog = null)
		{
            var wrapLog = parentLog?
                .AddChild(LogKey)
                .Call(nameof(ConfigureNewDataSource));

			newDs.ZoneId = appIdentity.ZoneId;
			newDs.AppId = appIdentity.AppId;
			if (upstream != null)
				((IDataTarget)newDs).Attach(upstream);
			if (configLookUp != null)
				newDs.Configuration.LookUps = configLookUp;

            if (parentLog != null) 
                newDs.InitLog(newDs.LogId, parentLog);
            wrapLog?.Invoke("ok");
        }


        /// <summary>
        /// Gets a DataSource with Query having PublishingFilter, ICache and IRootSource.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="showDrafts">Indicates whether Draft Entities should be returned</param>
        /// <param name="configProvider"></param>
        /// <param name="parentLog"></param>
        /// <returns>A single DataSource</returns>
        public static IDataSource GetPublishing(
            IAppIdentity app, 
            bool showDrafts = false, 
            ILookUpEngine configProvider = null, 
            ILog parentLog = null)
	    {
            var wrapLog = parentLog?
                .AddChild(LogKey)
                .Call(nameof(GetPublishing), $"#{app.ZoneId}/{app.AppId}, draft:{showDrafts}, config:{configProvider != null}");

			configProvider = configProvider ?? new LookUpEngine();

            var dataSource = GetDataSource(RootDataSource, app, null, configProvider, parentLog);

			var publishingFilter = GetDataSource<PublishingFilter>(app, dataSource, configProvider, parentLog);
			publishingFilter.ShowDrafts = showDrafts;

            wrapLog?.Invoke("ok");
            return publishingFilter;
		}

		///// <summary>
		///// Resolve and validate ZoneId and AppId for specified ZoneId and/or AppId (if any)
		///// </summary>
		///// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		//public static IAppIdentity  GetIdentity(int? zoneId, int? appId) =>
  //          zoneId != null && appId != null
  //              ? new AppIdentity(zoneId.Value, appId.Value)
  //              : Factory.GetAppsCache().GetIdentity(zoneId, appId);


        /// <summary>
        /// Get a new ICache DataSource
        /// </summary>
        /// <returns>A new IRootCache</returns>
        public static IAppRoot GetRootDs(IAppIdentity appIdentity, ILog parentLog = null) 
            => (IAppRoot)GetDataSource(RootDataSource, appIdentity, parentLog:parentLog);
    }

}