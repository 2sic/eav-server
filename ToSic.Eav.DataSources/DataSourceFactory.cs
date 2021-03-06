﻿using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources
{
    public class DataSourceFactory: HasLog<DataSourceFactory>
    {
        private readonly Lazy<ILookUpEngineResolver> _lookupResolveLazy;
        public IServiceProvider ServiceProvider { get; }

        #region Constructor / DI

        public DataSourceFactory(IServiceProvider serviceProvider, Lazy<ILookUpEngineResolver> lookupResolveLazy) : base($"{DataSourceConstants.LogPrefix}.Factry")
        {
            _lookupResolveLazy = lookupResolveLazy;
            ServiceProvider = serviceProvider;
        }

        #endregion

        #region GetDataSource 

        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="sourceName">Full Qualified Type/Interface Name</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="lookUps">Provides configuration values if needed</param>
        /// <returns>A single DataSource</returns>
        public IDataSource GetDataSource(string sourceName, IAppIdentity app, IDataSource upstream = null, ILookUpEngine lookUps = null)
        {
            var wrapLog = Log.Call(parameters: $"name: {sourceName}");
            // try to find with assembly name, or otherwise with GlobalName / previous names
            var type = Catalog.FindType(sourceName);

            // still not found? must show error
            if (type == null)
                throw new Exception("DataSource not installed on Server: " + sourceName);
            var result = GetDataSource(type, app, upstream, lookUps);
            wrapLog("ok");
            return result;
        }

        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="lookUps">Provides configuration values if needed</param>
        /// <returns>A single DataSource</returns>
        private IDataSource GetDataSource(Type type, IAppIdentity app, IDataSource upstream, ILookUpEngine lookUps)
        {
            var wrapLog = Log.Call();
            var newDs = ServiceProvider.Build<DataSourceBase>(type);
            ConfigureNewDataSource(newDs, app, upstream, lookUps);
            wrapLog("ok");
            return newDs;
        }


        #endregion

        #region GetDataSource Typed (preferred)

        public T GetDataSource<T>(IDataSource upstream) where T : IDataSource
            => GetDataSource<T>(upstream, upstream, upstream.Configuration.LookUpEngine);


        public T GetDataSource<T>(IAppIdentity appIdentity, IDataSource upstream, ILookUpEngine lookUps = null) where T : IDataSource
        {
            var wrapLog = Log.Call();

            if (upstream == null && lookUps == null)
                throw new Exception(
                    "Trying to GetDataSource<T> but cannot do so if both upstream and ConfigurationProvider are null.");

            var newDs = ServiceProvider.Build<T>();
            ConfigureNewDataSource(newDs, appIdentity, upstream, lookUps ?? upstream.Configuration.LookUpEngine);
            wrapLog("ok");
            return newDs;
        }

        #endregion

        #region Get Root Data Source with Publishing

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
            var wrapLog = Log.Call(parameters: $"#{app.Show()}, draft:{showDrafts}, config:{configProvider != null}");

            configProvider = configProvider ?? _lookupResolveLazy.Value.GetLookUpEngine(0);

            var dataSource = GetDataSource(DataSourceConstants.RootDataSource, app, null, configProvider);

            var publishingFilter = GetDataSource<PublishingFilter>(app, dataSource, configProvider);
            publishingFilter.ShowDrafts = showDrafts;

            wrapLog("ok");
            return publishingFilter;
        }

        #endregion


        #region Configure

        /// <summary>
        /// Helper function (internal) to configure a new data source. This code is used multiple times, that's why it's in an own function
        /// </summary>
        /// <param name="newSource">The new data source</param>
        /// <param name="appIdentity">app identifier</param>
        /// <param name="upstream">upstream data source - for auto-attaching</param>
        /// <param name="configLookUp">optional configuration provider - for auto-attaching</param>
        private void ConfigureNewDataSource<T>(
            T newSource,
            IAppIdentity appIdentity,
            IDataSource upstream = null,
            ILookUpEngine configLookUp = null) where T : IDataSource
        {
            var wrapLog = Log.Call($"DataSource {newSource.Name} ({newSource.LogId})");
            if (!(newSource is DataSourceBase newDs))
            {
                wrapLog("can't configure, not a base source");
                return;
            }

            // attach this factory, for re-use inside the data source
            newDs.DataSourceFactory = this;

            newDs.ZoneId = appIdentity.ZoneId;
            newDs.AppId = appIdentity.AppId;
            if (upstream != null)
                ((IDataTarget)newDs).Attach(upstream);
            if (configLookUp != null) 
                newDs.Init(configLookUp);

            newDs.InitLog(newDs.LogId, Log);
            wrapLog("ok");
        }

        #endregion
    }
}
