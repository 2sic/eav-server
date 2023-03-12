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

        public DataSourceFactory DataSourceFactory => _dataSourceFactory.Get(GetService<DataSourceFactory>);
        private readonly GetOnce<DataSourceFactory> _dataSourceFactory = new GetOnce<DataSourceFactory>();

        public T CreateDataSource<T>(IDataSource upstream) where T: IDataSource => DataSourceFactory.Create<T>(upstream: upstream);
        public T CreateDataSource<T>(IDataStream upstream) where T : IDataSource => DataSourceFactory.Create<T>(upstream);

        public T CreateDataSource<T>(ILookUpEngine lookUpEngine = default) where T : IDataSource
        {
            return DataSourceFactory.Create<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));
        }
    }
}
