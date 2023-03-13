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

        private readonly IServiceProvider _serviceProvider;
        private readonly LazySvc<ILookUpEngineResolver> _lookupResolveLazy;
        
        public DataSourceFactory(IServiceProvider serviceProvider,
            LazySvc<ILookUpEngineResolver> lookupResolveLazy) : base($"{DataSourceConstants.LogPrefix}.Factry")
        {
            ConnectServices(
                _serviceProvider = serviceProvider,
                _lookupResolveLazy = lookupResolveLazy
            );
        }

        #endregion

        #region GetDataSource 

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
            var newDs = _serviceProvider.Build<IDataSource>(type, Log);
            return newDs.Init(appIdentity: appIdentity, upstream: upstream, lookUp: lookUps);
        });


        #endregion

        #region GetDataSource Typed


        /// <summary>
        /// Experimental 12.10
        /// </summary>
        /// <typeparam name="TDataSource"></typeparam>
        /// <param name="upstream"></param>
        /// <returns></returns>
        [PrivateApi("internal, experimental, only used in tests ATM")]
        public TDataSource Create<TDataSource>(IDataStream upstream) where TDataSource : IDataSource
        {
            if (upstream.Source == null)
                throw new Exception("Unexpected source - stream without a real source. can't process; wip");
            var source = upstream.Source;
            var ds = Create<TDataSource>(appIdentity: source, configLookUp: source.Configuration.LookUpEngine);
            if (!(ds is IDataTarget target))
                throw new Exception("error, ds not target; wip");
            target.Attach(DataSourceConstants.DefaultStreamName, upstream);
            return ds;
        }

        public TDataSource Create<TDataSource>(
            string noParamOrder = Parameters.Protector,
            IDataSource upstream = default,
            IAppIdentity appIdentity = default,
            ILookUpEngine configLookUp = default) where TDataSource : IDataSource => Log.Func(() =>
        {
            if (upstream == null && appIdentity == null)
                throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(upstream)} and {nameof(appIdentity)} no not be null.");
            if (upstream == null && configLookUp == null)
                throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(upstream)} and {nameof(configLookUp)} no not be null.");

            var newDs = _serviceProvider.Build<TDataSource>(Log);
            return newDs.Init(appIdentity: appIdentity, upstream: upstream, lookUp: configLookUp);
        });

        #endregion

        #region Get Root Data Source with Publishing

        /// <summary>
        /// Gets a DataSource with Query having PublishingFilter, ICache and IRootSource.
        /// </summary>
        /// <param name="appIdentity"></param>
        /// <param name="showDrafts">Indicates whether Draft Entities should be returned</param>
        /// <param name="configLookUp"></param>
        /// <returns>A single DataSource</returns>
        public IDataSource GetPublishing(
            IAppIdentity appIdentity,
            // TODO: FIGURE out way to not usually specify this, so it's not retrieved everywhere but automatically in GetLookupEngine if not specified
            bool showDrafts = false,
            ILookUpEngine configLookUp = null) => Log.Func( $"#{appIdentity.Show()}, draft:{showDrafts}, config:{configLookUp != null}", () =>
        {
            configLookUp = configLookUp ?? _lookupResolveLazy.Value.GetLookUpEngine(0);

            var appRoot = Create<IAppRoot>(appIdentity: appIdentity, configLookUp: configLookUp);

            var publishingFilter = Create<PublishingFilter>(upstream: appRoot, configLookUp: configLookUp);
            publishingFilter.ShowDrafts = showDrafts;

            return publishingFilter;
        });

        #endregion
    }
}
