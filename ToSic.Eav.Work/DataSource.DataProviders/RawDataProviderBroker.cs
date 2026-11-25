namespace ToSic.Eav.DataSource.DataProviders;

/// <summary>
/// Provides raw data by resolving named raw data providers from an application work chain and delegating requests to
/// the resolved provider.
/// </summary>
/// <remarks>
/// If no provider is registered for a requested name,
/// processing falls back to `RawDataProviderError`
/// so the request still returns a result sequence containing error information.
/// </remarks>
/// <param name="providerGenerator">Underlying data provider generator - to be chained / updated with the current context.</param>
public class RawDataProviderBroker(AppDataChain<IRawDataProviderWithCtx> providerGenerator)
    : ServiceWithSetup<IAppDataContext>("Dta.RawPrv", connect: [providerGenerator])
{
    public IEnumerable<IRawData> Provide(string name, IDataSourceConfiguration configuration)
    {
        var l = Log.Fn<IEnumerable<IRawData>>($"Broker for '{name}'");

        // Try to find the keyed raw data provider from DI
        // if not found, fall back to a provider which simply provides an error message
        var provider = providerGenerator.TryNew(name, MyOptions);
        if (provider == null)
            return l.Return(
                new RawDataProviderError([$"Failed to create raw data provider for {name}"])
                    .Provide(configuration));

        try
        {
            // Return inner result
            var result = provider.Provide(configuration).ToListOpt();
            return l.Return(result, $"{result.Count}");
        }
        catch (Exception ex)
        {
            // Return an error
            return l.Return(new RawDataProviderError(
                [
                    $"Error using data provider {name}",
                    ex.Message,
                ])
                .Provide(configuration));
        }
    }
}