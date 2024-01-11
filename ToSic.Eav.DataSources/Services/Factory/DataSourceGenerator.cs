namespace ToSic.Eav.Services;

internal class DataSourceGenerator<TDataSource>: ServiceBase, IDataSourceGenerator<TDataSource> where TDataSource : IDataSource
{
    private readonly LazySvc<IDataSourcesService> _dataSourceFactory;

    public DataSourceGenerator(LazySvc<IDataSourcesService> dataSourceFactory): base("DS.DsGen")
    {
        _dataSourceFactory = dataSourceFactory;
    }

    public TDataSource New(IDataSourceLinkable attach = default, IDataSourceOptions options = default) =>
        _dataSourceFactory.Value.Create<TDataSource>(attach: attach, options: options);
}