namespace ToSic.Eav.DataSource.Linking;

internal class DsLinkingTestData
{
    public static IDataSourceLink GetWithNull() => new DataSourceLink
    {
        DataSource = null!
    };

    public static IDataSourceLink GetWithMockDataSource(string? name = null) => new DataSourceLink
    {
        DataSource = new DataSourceMock { Name = name ?? DataSourceMock.DefaultName }
    };

    public const string Rename = "Something";
    public const string Rename2 = "SomethingElse";
    public const string Rename3 = "Something3";

}
