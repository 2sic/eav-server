using System;
using ToSic.Eav.Apps;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public class DataSourceFactory: ServiceBase, IDataSourceFactory
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

        /// <inheritdoc />
        public IDataSource Create(
            Type type,
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IDataSourceConfiguration configuration = default) => Log.Func(() =>
        {
            var newDs = _serviceProvider.Build<IDataSource>(type, Log);
            return newDs.Init(source: source, configuration: configuration);
        });


        #endregion

        #region GetDataSource Typed


        /// <summary>
        /// Experimental 12.10
        /// </summary>
        /// <typeparam name="TDataSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [PrivateApi("internal, experimental, only used in tests ATM")]
        public TDataSource Create<TDataSource>(IDataStream source) where TDataSource : IDataSource
        {
            if (source.Source == null)
                throw new Exception("Unexpected source - stream without a real source. can't process; wip");
            var sourceDs = source.Source;
            var ds = Create<TDataSource>(configuration: new DataSourceConfiguration(appIdentity: sourceDs, lookUp: sourceDs.Configuration.LookUpEngine));
            ds.Attach(DataSourceConstants.StreamDefaultName, source);
            return ds;
        }

        /// <inheritdoc />
        public TDataSource Create<TDataSource>(
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IDataSourceConfiguration configuration = default) where TDataSource : IDataSource => Log.Func(() =>
        {
            if (source == null && configuration?.AppIdentity == null)
                throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(source)} and configuration.AppIdentity no not be null.");
            if (source == null && configuration?.LookUp == null)
                throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(source)} and configuration.LookUp no not be null.");

            var newDs = _serviceProvider.Build<TDataSource>(Log);
            return newDs.Init(source: source, configuration: configuration);
        });

        #endregion

        #region Get Root Data Source with Publishing
        
        /// <inheritdoc />
        public IDataSource CreateDefault(
            IDataSourceConfiguration configuration,
            string noParamOrder = Parameters.Protector)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var appIdentity = configuration.AppIdentity
                              ?? throw new ArgumentNullException(nameof(IDataSourceConfiguration.AppIdentity));
            var lookUp = configuration.LookUp;
            var l = Log.Fn<IDataSource>($"#{appIdentity.Show()}, draft:{configuration.ShowDrafts}, lookUp:{lookUp != null}");

            configuration = lookUp != null 
                ? configuration 
                : new DataSourceConfiguration(configuration, lookUp: _lookupResolveLazy.Value.GetLookUpEngine(0));
            var appRoot = Create<IAppRoot>(configuration: configuration);
            var publishingFilter = Create<PublishingFilter>(source: appRoot, configuration: configuration);

            if (configuration.ShowDrafts != null)
                publishingFilter.ShowDrafts = configuration.ShowDrafts;

            return l.Return(publishingFilter, "ok");
        }

        #endregion
    }
}
