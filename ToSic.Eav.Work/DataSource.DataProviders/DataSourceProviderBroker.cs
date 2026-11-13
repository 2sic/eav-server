namespace ToSic.Eav.DataSource.DataProviders;

public class DataSourceProviderBroker(AppWorkChain<IDataSourceRawDataProviderWithCtx> data)
    : ServiceWithSetup<IAppWorkContext>("Dta.RawPrv", connect: [data])
{
    public IEnumerable<IRawData> Provide(string name, IDataSourceConfiguration configuration)
    {
        return (data.TryNew(name, MyOptions) ?? new DataSourceProviderRawShowingError(name))
            .Provide(configuration);
    }


    
}
internal class DataSourceProviderRawShowingError(string name) : IDataSourceRawDataProviderWithCtx
{
    public void Setup(IAppWorkContext options) { /*ignore*/ }

    public IEnumerable<IRawData> Provide(IDataSourceConfiguration configuration)
        => [new RawProviderError($"Failed to create raw data provider for {name}")];

    private record RawProviderError(string Title) : IRawEntityAutoConvert;
}
