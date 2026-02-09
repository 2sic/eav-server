using static ToSic.Eav.DataSource.DataSourceConstants;
using static ToSic.Eav.DataSource.Linking.DsLinkingTestData;

namespace ToSic.Eav.DataSource.Linking;

public class DsLinkingBasics
{
    [Fact]
    public void OutNameIsDefault() =>
        Equal(StreamDefaultName, GetWithNull().OutNameTac());

    [Fact]
    public void InNameIsDefault() =>
        Equal(StreamDefaultName, GetWithNull().InNameTac());

    [Fact]
    public void EmptyLink() =>
        NotNull(GetWithNull());

    [Fact]
    public void MockDataSourceLink() =>
        NotNull(GetWithMockDataSource());



}
