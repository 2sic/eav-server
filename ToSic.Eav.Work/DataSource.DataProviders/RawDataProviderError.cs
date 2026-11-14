namespace ToSic.Eav.DataSource.DataProviders;

/// <summary>
/// Simple raw data provider which will simply show an error message.
/// </summary>
/// <remarks>
/// Not meant for dependency injection, as it just creates pure messages which are specified in the constructor.
/// This is used to return an error message when a data provider can't be created or used.
/// </remarks>
/// <param name="messages">List of messages</param>
internal class RawDataProviderError(IEnumerable<string> messages) : IRawDataProviderWithCtx
{
    public void Setup(IAppWorkContext options) { /*ignore*/ }

    public IEnumerable<IRawData> Provide(IDataSourceConfiguration configuration)
        => messages.Select(m => new RawProviderError(m));

    /// <summary>
    /// The trivial record containing just a title.
    /// </summary>
    /// <param name="Title"></param>
    private record RawProviderError(string Title) : IRawEntityAutoConvert;
}