using System;
using ToSic.Eav.Apps;
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

        ///// <summary>
        ///// Get DataSource for specified sourceName/Type
        ///// </summary>
        ///// <param name="assemblyAndType">Full Qualified Type/Interface Name</param>
        ///// <param name="appIdentity"></param>
        ///// <param name="upstream">In-Connection</param>
        ///// <param name="lookUps">Provides configuration values if needed</param>
        ///// <returns>A single DataSource</returns>
        //public IDataSource Create(string assemblyAndType, IAppIdentity appIdentity, IDataSource upstream = null, ILookUpEngine lookUps = null
        //) => Log.Func($"name: {assemblyAndType}", () =>
        //{
        //    // try to find with assembly name, or otherwise with GlobalName / previous names
        //    var type = DataSourceCatalog.FindType(assemblyAndType);

        //    // still not found? must return an Error DataSource
        //    if (type == null)
        //    {
        //        var errDs = _errDsGenerator.New();
        //        errDs.Title = "DataSource not found";
        //        errDs.Message = $"DataSource '{assemblyAndType}' is not installed on Server. You should probably install it in the CMS.";
        //        return errDs;
        //    }
        //    var result = Create(type, appIdentity, upstream, lookUps);
        //    return result;
        //});

        /// <summary>
        /// Get DataSource for specified sourceName/Type
        /// </summary>
        /// <param name="type">the .net type of this data-source</param>
        /// <param name="appIdentity"></param>
        /// <param name="upstream">In-Connection</param>
        /// <param name="lookUps">Provides configuration values if needed</param>
        /// <returns>A single DataSource</returns>
        public IDataSource Create(Type type, IAppIdentity appIdentity, IDataSource upstream, ILookUpEngine lookUps) => Log.Func(() =>
        {
            var newDs = _serviceProvider.Build<DataSource>(type, Log);
            return newDs.Init(appIdentity: appIdentity, upstream: upstream, lookUp: lookUps);
        });


        #endregion

        #region GetDataSource Typed

        public TDataSource GetDataSource<TDataSource>(IDataSource upstream) where TDataSource : IDataSource
            => GetDataSource<TDataSource>(upstream, upstream, upstream.Configuration.LookUpEngine);

        /// <summary>
        /// Experimental 12.10
        /// </summary>
        /// <typeparam name="TDataSource"></typeparam>
        /// <param name="upstream"></param>
        /// <returns></returns>
        [PrivateApi("internal, experimental")]
        public TDataSource GetDataSource<TDataSource>(IDataStream upstream) where TDataSource : IDataSource
        {
            if (upstream.Source == null)
                throw new Exception("Unexpected source - stream without a real source. can't process; wip");
            var source = upstream.Source;
            var ds = GetDataSource<TDataSource>(source, null, source.Configuration.LookUpEngine);
            if (!(ds is IDataTarget target))
                throw new Exception("error, ds not target; wip");
            target.Attach(DataSourceConstants.DefaultStreamName, upstream);
            return ds;
        }

        public TDataSource GetDataSource<TDataSource>(IAppIdentity appIdentity, IDataSource upstream, ILookUpEngine lookUps = null) where TDataSource : IDataSource => Log.Func(() =>
        {
            if (upstream == null && lookUps == null)
                throw new Exception($"Can't get GetDataSource<T> because both {nameof(upstream)} and {nameof(lookUps)} are null.");

            var newDs = _serviceProvider.Build<TDataSource>(Log);
            return newDs.Init(appIdentity: appIdentity, upstream: upstream, lookUp: lookUps ?? upstream.Configuration.LookUpEngine);
        });

        #endregion

        #region Get Root Data Source with Publishing

        /// <summary>
        /// Gets a DataSource with Query having PublishingFilter, ICache and IRootSource.
        /// </summary>
        /// <param name="appIdentity"></param>
        /// <param name="showDrafts">Indicates whether Draft Entities should be returned</param>
        /// <param name="configProvider"></param>
        /// <returns>A single DataSource</returns>
        public IDataSource GetPublishing(
            IAppIdentity appIdentity,
            bool showDrafts = false,
            ILookUpEngine configProvider = null) => Log.Func( $"#{appIdentity.Show()}, draft:{showDrafts}, config:{configProvider != null}", () =>
        {
            configProvider = configProvider ?? _lookupResolveLazy.Value.GetLookUpEngine(0);

            var appRoot = GetDataSource<IAppRoot>(appIdentity, null, configProvider);

            var publishingFilter = GetDataSource<PublishingFilter>(appIdentity, appRoot, configProvider);
            publishingFilter.ShowDrafts = showDrafts;

            return publishingFilter;
        });

        #endregion
    }
}
