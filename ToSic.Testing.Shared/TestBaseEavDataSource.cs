using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Eav.Services;
using ToSic.Lib.Helpers;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEavDataSource(EavTestConfig testConfig = default)
    : TestBaseEavCore(testConfig)
{
    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services
            // DataSources
            .AddDataSources();
    }

    public IDataSourcesService DataSourceFactory => _dataSourceFactory.Get(GetService<IDataSourcesService>);
    private readonly GetOnce<IDataSourcesService> _dataSourceFactory = new GetOnce<IDataSourcesService>();

    public T CreateDataSource<T>(IDataSource upstream) where T: IDataSource => DataSourceFactory.TestCreate<T>(upstream: upstream);

    public T CreateDataSource<T>(IDataStream upstream, object options = default) where T : IDataSource 
        => ((DataSourcesService)DataSourceFactory).Create<T>(stream: upstream, new DataSourceOptionConverter().Convert(options, false, false));
    public T CreateDataSource<T>(IDataSourceLinkable attach, object options = default) where T : IDataSource 
        => ((DataSourcesService)DataSourceFactory).Create<T>(attach: attach, new DataSourceOptionConverter().Convert(options, false, false));

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