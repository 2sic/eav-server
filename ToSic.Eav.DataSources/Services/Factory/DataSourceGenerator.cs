using ToSic.Eav.DataSource;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Services;

public class DataSourceGenerator<TDataSource>: ServiceBase, IDataSourceGenerator<TDataSource> where TDataSource : IDataSource
{
    private readonly LazySvc<DataSourcesService> _dataSourceFactory;

    public DataSourceGenerator(LazySvc<DataSourcesService> dataSourceFactory): base("DS.DsGen")
    {
        _dataSourceFactory = dataSourceFactory;
    }

    public TDataSource New(IDataSourceLinkable attach = default, IDataSourceOptions options = default) =>
        _dataSourceFactory.Value.Create<TDataSource>(attach: attach, options: options);
}