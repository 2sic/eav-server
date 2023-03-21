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
            IAppIdentity appIdentity = default,
            ILookUpEngine configSource = default) => Log.Func(() =>
        {
            var newDs = _serviceProvider.Build<IDataSource>(type, Log);
            return newDs.Init(appIdentity: appIdentity, source: source, configSource: configSource);
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
            var ds = Create<TDataSource>(appIdentity: source, configSource: source.Configuration.LookUpEngine);
            ds.Attach(DataSourceConstants.StreamDefaultName, upstream);
            return ds;
        }

        /// <inheritdoc />
        public TDataSource Create<TDataSource>(
            string noParamOrder = Parameters.Protector,
            IDataSource source = default,
            IAppIdentity appIdentity = default,
            ILookUpEngine configSource = default) where TDataSource : IDataSource => Log.Func(() =>
        {
            if (source == null && appIdentity == null)
                throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(source)} and {nameof(appIdentity)} no not be null.");
            if (source == null && configSource == null)
                throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(source)} and {nameof(configSource)} no not be null.");

            var newDs = _serviceProvider.Build<TDataSource>(Log);
            return newDs.Init(appIdentity: appIdentity, source: source, configSource: configSource);
        });

        #endregion

        #region Get Root Data Source with Publishing

        /// <inheritdoc />
        public IDataSource CreateDefault(
            IAppIdentity appIdentity,
            string noParamOrder = Parameters.Protector,
            bool? showDrafts = default, 
            ILookUpEngine configSource = default) 
        {
            var l = Log.Fn<IDataSource>($"#{appIdentity.Show()}, draft:{showDrafts}, config:{configSource != null}");

            configSource = configSource ?? _lookupResolveLazy.Value.GetLookUpEngine(0);
            var appRoot = Create<IAppRoot>(appIdentity: appIdentity, configSource: configSource);
            var publishingFilter = Create<PublishingFilter>(source: appRoot, configSource: configSource);

            if (showDrafts != null)
                publishingFilter.ShowDrafts = showDrafts;

            return l.Return(publishingFilter, "ok");
        }

        #endregion
    }
}
