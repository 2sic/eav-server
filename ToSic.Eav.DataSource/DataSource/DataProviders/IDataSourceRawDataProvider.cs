using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSource.DataProviders;

/// <summary>
/// Data provider for data sources - named.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDataSourceRawDataProvider
{
    public IEnumerable<IRawData> Provide(IDataSourceConfiguration configuration);
}