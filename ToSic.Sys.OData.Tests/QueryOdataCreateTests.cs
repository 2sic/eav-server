namespace ToSic.Sys.OData.Tests;

public class QueryOdataCreateTests
{
    private static ODataOptions CreateWithAllValues(string? name = null)
    {
        return QueryODataParams.Create(v => v, name);
    }

    private static ODataOptions CreateWithNoValues() =>
        QueryODataParams.Create(v => v.ToDictionary(
                pair => pair.Key,
                pair => ""),
            null
        );

    private static ODataOptions CreateSelectOnly() =>
        QueryODataParams.Create(v => v.ToDictionary(
                pair => pair.Key,
                pair => pair.Key == ODataConstants.SelectParamName ? "test" : ""),
            null
        );


    [Fact]
    public void CreateAllValuesHasAllValues() =>
        Equal(QueryODataParams.ODataParams.Count, CreateWithAllValues().AllRawTac.Count);

    [Fact]
    public void CreateAllValuesIsNotEmpty() =>
        False(CreateWithAllValues().IsEmpty());

    [Fact]
    public void CreateAllValuesIsNotEmptyExceptForSelect() =>
        False(CreateWithAllValues().IsEmptyExceptForSelect());



    [Theory]
    [InlineData(null)]
    [InlineData("Default")]
    public void CreateAllValuesUnchanged(string? name) =>
        Equal(QueryODataParams.ODataParams, CreateWithAllValues(name).AllRawTac);

    [Fact]
    public void CreateAllValuesWithNameChanged() =>
        NotEqual(QueryODataParams.ODataParams, CreateWithAllValues("Test").AllRawTac);

    [Theory]
    [InlineData("Test")]
    [InlineData("Books")]
    [InlineData("Authors")]
    public void CreateAllValuesWithNameEveryValueContainsKey(string name) =>
        Equal(QueryODataParams.ODataParams.Count, CreateWithAllValues(name).AllRawTac.Count(pair => pair.Value.Contains($":{name}")));



    [Fact]
    public void CreateNoValuesHasAllValues() =>
        Empty(CreateWithNoValues().AllRawTac);

    [Fact]
    public void CreateNoValuesIsEmpty() =>
        True(CreateWithNoValues().IsEmpty());

    [Fact]
    public void CreateNoValuesIsNotEmptyExceptForSelect() =>
        True(CreateWithNoValues().IsEmptyExceptForSelect());



    [Fact]
    public void CreateSelectOnlyHasAllValues() =>
        Single(CreateSelectOnly().AllRawTac);

    [Fact]
    public void CreateSelectOnlyIsEmpty() =>
        False(CreateSelectOnly().IsEmpty());

    [Fact]
    public void CreateSelectOnlyIsNotEmptyExceptForSelect() =>
        True(CreateSelectOnly().IsEmptyExceptForSelect());

}
