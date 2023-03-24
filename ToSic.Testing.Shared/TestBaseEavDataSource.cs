using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Lib.Helpers;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseEavDataSource: TestBaseEavCore
    {
        protected TestBaseEavDataSource(TestConfiguration testConfiguration = default) : base(testConfiguration)
        {
        }

        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            services
                // DataSources
                .AddDataSources();
        }

        public IDataSourceFactory DataSourceFactory => _dataSourceFactory.Get(GetService<IDataSourceFactory>);
        private readonly GetOnce<IDataSourceFactory> _dataSourceFactory = new GetOnce<IDataSourceFactory>();

        public T CreateDataSource<T>(IDataSource upstream) where T: IDataSource => DataSourceFactory.TestCreate<T>(upstream: upstream);

        public T CreateDataSource<T>(IDataStream upstream, object options = default) where T : IDataSource 
            => ((DataSourceFactory)DataSourceFactory).Create<T>(source: upstream, new DataSourceOptions.Converter().Convert(options, false, false));

        public T CreateDataSource<T>(ILookUpEngine lookUpEngine = default) where T : IDataSource
        {
            return DataSourceFactory.TestCreate<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));
        }
        public T CreateDataSourceNew<T>(ILookUpEngine lookUpEngine = default, object options = default) where T : IDataSource
        {
            return DataSourceFactory.TestCreate<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));
        }
        public T CreateDataSourceNew<T>(object options = default) where T : IDataSource
        {
            return DataSourceFactory.TestCreateNew<T>(appIdentity: new AppIdentity(0, 0), options: options);
        }
    }
}
