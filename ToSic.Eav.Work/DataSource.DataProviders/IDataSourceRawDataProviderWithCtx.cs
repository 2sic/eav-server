namespace ToSic.Eav.DataSource.DataProviders;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDataSourceRawDataProviderWithCtx: IServiceWithSetup<IAppWorkContext>
{
    public IEnumerable<IRawData> Provide(IDataSourceConfiguration configuration);
}