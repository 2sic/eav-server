namespace ToSic.Eav.Services;

internal class DataSourceGenerator<TDataSource>(LazySvc<IDataSourcesService> dataSourceFactory)
    : ServiceBase("DS.DsGen"), IDataSourceGenerator<TDataSource>
    where TDataSource : IDataSource
{
    public TDataSource New(IDataSourceOptions? options = default)
        => dataSourceFactory.Value.Create<TDataSource>(options: options);

    [Obsolete("v21")]
    public TDataSource New(IDataSourceLinkable? attach = default, IDataSourceOptions? options = default)
        => dataSourceFactory.Value.Create<TDataSource>(/*attach: attach,*/ options: options.WithAttach(attach));
}