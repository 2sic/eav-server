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
	public class DataSource: HasLog
	{
	    private const string LogKey = "DS.Factry";
        private static readonly string RootDataSource = typeof(IAppRoot).AssemblyQualifiedName;

        public DataSource(ILog parentLog = null) : base(LogKey, parentLog) { }

        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="sourceName">Full Qualified Type/Interface Name</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="configLookUp">Provides configuration values if needed</param>
        /// <returns>A single DataSource</returns>
        public IDataSource GetDataSource(string sourceName, IAppIdentity app, IDataSource upstream = null, ILookUpEngine configLookUp = null)
        {
            var wrapLog = Log.Call(parameters: $"name: {sourceName}");
		    // try to find with assembly name, or otherwise with GlobalName / previous names
            var type = Catalog.FindType(sourceName);

		    // still not found? must show error
			if (type == null)
			    throw new Exception("DataSource not installed on Server: " + sourceName);
            var result = GetDataSource(type, app, upstream, configLookUp);
            wrapLog("ok");
            return result;
        }


        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="configLookUp">Provides configuration values if needed</param>
        /// <returns>A single DataSource</returns>
        private IDataSource GetDataSource(
            Type type, 
            IAppIdentity app, 
            IDataSource upstream,
	        ILookUpEngine configLookUp)
        {
            var wrapLog = Log.Call();
	        var newDs = (DataSourceBase) Factory.Resolve(type);
            ConfigureNewDataSource(newDs, app, upstream, configLookUp);
            wrapLog("ok");
            return newDs;
	    }

        public T GetDataSource<T>(
            IDataSource upstream) where T : IDataSource 
            => GetDataSource<T>(upstream, upstream, upstream.Configuration.LookUps);


        public T GetDataSource<T>(
            IAppIdentity appIdentity, 
            IDataSource upstream, 
            ILookUpEngine configLookUp = null) where T : IDataSource
        {
            var wrapLog = Log.Call();

            if (upstream == null && configLookUp == null)
                throw new Exception(
                    "Trying to GetDataSource<T> but cannot do so if both upstream and ConfigurationProvider are null.");

            var newDs = (DataSourceBase) Factory.Resolve(typeof(T));
            ConfigureNewDataSource(newDs, appIdentity, upstream, configLookUp ?? upstream.Configuration.LookUps);
            wrapLog("ok");
            return (T) Convert.ChangeType(newDs, typeof(T));
        }

        /// <summary>
        /// Helper function (internal) to configure a new data source. This code is used multiple times, that's why it's in an own function
        /// </summary>
        /// <param name="newDs">The new data source</param>
        /// <param name="appIdentity">app identifier</param>
        /// <param name="upstream">upstream data source - for auto-attaching</param>
        /// <param name="configLookUp">optional configuration provider - for auto-attaching</param>
        private void ConfigureNewDataSource(
            DataSourceBase newDs, 
            IAppIdentity appIdentity,
			IDataSource upstream = null,
			ILookUpEngine configLookUp = null)
        {
            var wrapLog = Log.Call();

			newDs.ZoneId = appIdentity.ZoneId;
			newDs.AppId = appIdentity.AppId;
			if (upstream != null)
				((IDataTarget)newDs).Attach(upstream);
			if (configLookUp != null)
				newDs.Configuration.LookUps = configLookUp;

            newDs.InitLog(newDs.LogId, Log);
            wrapLog("ok");
        }


        /// <summary>
        /// Gets a DataSource with Query having PublishingFilter, ICache and IRootSource.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="showDrafts">Indicates whether Draft Entities should be returned</param>
        /// <param name="configProvider"></param>
        /// <returns>A single DataSource</returns>
        public IDataSource GetPublishing(
            IAppIdentity app, 
            bool showDrafts = false, 
            ILookUpEngine configProvider = null)
        {
            var wrapLog = Log.Call(parameters: $"#{app.ZoneId}/{app.AppId}, draft:{showDrafts}, config:{configProvider != null}");

			configProvider = configProvider ?? new LookUpEngine(Log);

            var dataSource = GetDataSource(RootDataSource, app, null, configProvider);

			var publishingFilter = GetDataSource<PublishingFilter>(app, dataSource, configProvider);
			publishingFilter.ShowDrafts = showDrafts;

            wrapLog("ok");
            return publishingFilter;
		}


        /// <summary>
        /// Get a new ICache DataSource
        /// </summary>
        /// <returns>A new IRootCache</returns>
        public IAppRoot GetRootDs(IAppIdentity appIdentity) 
            => (IAppRoot)GetDataSource(RootDataSource, appIdentity);
    }

}