using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public class DataSourceFactory: ServiceBase
    {
        #region Constructor / DI

        private readonly Generator<Error> _errDsGenerator;
        private readonly IServiceProvider _serviceProvider;
        private readonly LazySvc<ILookUpEngineResolver> _lookupResolveLazy;
        
        public DataSourceFactory(IServiceProvider serviceProvider,
            LazySvc<ILookUpEngineResolver> lookupResolveLazy,
            Generator<Error> errDsGenerator) : base($"{DataSourceConstants.LogPrefix}.Factry")
        {
            ConnectServices(
                _serviceProvider = serviceProvider,
                _lookupResolveLazy = lookupResolveLazy,
                _errDsGenerator = errDsGenerator
            );
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
        public IDataSource GetDataSource(string sourceName, IAppIdentity app, IDataSource upstream = null, ILookUpEngine lookUps = null
        ) => Log.Func($"name: {sourceName}", () =>
        {
            // try to find with assembly name, or otherwise with GlobalName / previous names
            var type = DataSourceCatalog.FindType(sourceName);

            // still not found? must return an Error DataSource
            if (type == null)
            {
                var errDs = _errDsGenerator.New();
                errDs.Title = "DataSource not found";
                errDs.Message = $"DataSource '{sourceName}' is not installed on Server. You should probably install it in the CMS.";
                return errDs;
            }
            var result = GetDataSource(type, app, upstream, lookUps);
            return result;
        });

        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="app"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="lookUps">Provides configuration values if needed</param>
        /// <returns>A single DataSource</returns>
        private IDataSource GetDataSource(Type type, IAppIdentity app, IDataSource upstream, ILookUpEngine lookUps) => Log.Func(() =>
        {
            var newDs = _serviceProvider.Build<DataSource>(type, Log);
            ConfigureNewDataSource(newDs, app, upstream, lookUps);
            return newDs;
        });


        #endregion

        #region GetDataSource Typed (preferred)

        public T GetDataSource<T>(IDataSource upstream) where T : IDataSource
            => GetDataSource<T>(upstream, upstream, upstream.Configuration.LookUpEngine);

        /// <summary>
        /// Experimental 12.10
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="upstream"></param>
        /// <returns></returns>
        [PrivateApi("internal, experimental")]
        public T GetDataSource<T>(IDataStream upstream) where T : IDataSource
        {
            if (upstream.Source == null)
                throw new Exception("Unexpected source - stream without a real source. can't process; wip");
            var source = upstream.Source;
            var ds = GetDataSource<T>(source, null, source.Configuration.LookUpEngine);
            if (!(ds is IDataTarget target))
                throw new Exception("error, ds not target; wip");
            target.Attach(Constants.DefaultStreamName, upstream);
            return ds;
        }

        public T GetDataSource<T>(IAppIdentity appIdentity, IDataSource upstream, ILookUpEngine lookUps = null) where T : IDataSource => Log.Func(() =>
        {
            if (upstream == null && lookUps == null)
                throw new Exception("Can't get GetDataSource<T> because both upstream and lookUps are null.");

            var newDs = _serviceProvider.Build<T>(Log);
            ConfigureNewDataSource(newDs, appIdentity, upstream, lookUps ?? upstream.Configuration.LookUpEngine);
            return newDs;
        });

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
            ILookUpEngine configProvider = null) => Log.Func( $"#{app.Show()}, draft:{showDrafts}, config:{configProvider != null}", () =>
        {
            configProvider = configProvider ?? _lookupResolveLazy.Value.GetLookUpEngine(0);

            var dataSource = GetDataSource(DataSourceConstants.RootDataSource, app, null, configProvider);

            var publishingFilter = GetDataSource<PublishingFilter>(app, dataSource, configProvider);
            publishingFilter.ShowDrafts = showDrafts;

            return publishingFilter;
        });

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
            ILookUpEngine configLookUp = null) where T : IDataSource => Log.Do($"DataSource {newSource.Name}", () =>
        {
            if (!(newSource is DataSource newDs))
                return "can't configure, not a base source";

            newDs.ZoneId = appIdentity.ZoneId;
            newDs.AppId = appIdentity.AppId;
            if (upstream != null)
                ((IDataTarget)newDs).Attach(upstream);
            if (configLookUp != null)
                newDs.Init(configLookUp);
            return "ok";
        });

        #endregion
    }
}
