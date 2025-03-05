using ToSic.Eav.Apps;
using ToSic.Eav.DataSource;
using ToSic.Eav.LookUp;
using ToSic.Eav.Services;
using ToSic.Lib.Services;

namespace ToSic.Testing.Shared;

public class DataSourcesTstBuilder(IDataSourcesService dataSourcesService): ServiceBase("Tst.DsFHlp", connect: [dataSourcesService])
{
    public IDataSourcesService DataSourceSvc => dataSourcesService;

    public T CreateDataSource<T>(ILookUpEngine lookUpEngine = default) where T : IDataSource
        => dataSourcesService.TestCreate<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));

    public T CreateDataSource<T>(IDataSource upstream) where T : IDataSource
        => dataSourcesService.TestCreate<T>(upstream: upstream);

    public T CreateDataSource<T>(IDataStream upstream, object options = default) where T : IDataSource
        => ((DataSourcesService)dataSourcesService).Create<T>(stream: upstream, new DataSourceOptionConverter().Convert(options, false, false));

    public T CreateDataSource<T>(IDataSourceLinkable attach, object options = default) where T : IDataSource
        => dataSourcesService.Create<T>(attach: attach, new DataSourceOptionConverter().Convert(options, false, false));



    public T CreateDataSourceNew<T>(ILookUpEngine lookUpEngine = default, object options = default) where T : IDataSource
        => dataSourcesService.TestCreate<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));

    public T CreateDataSourceNew<T>(object options = default) where T : IDataSource
        => dataSourcesService.TestCreateNew<T>(appIdentity: new AppIdentity(0, 0), options: options);

}