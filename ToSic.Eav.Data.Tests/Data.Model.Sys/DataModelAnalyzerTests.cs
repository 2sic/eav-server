using ToSic.Eav.Model.Sys;

namespace ToSic.Eav.Data.Model.Sys;

public class DataModelAnalyzerTests
{
    [Theory]
    // isInterface = false
    [InlineData(null, false, new string[0])]
    [InlineData("", false, new string[0])]
    [InlineData("Thing", false, new[] { "Thing" })]
    [InlineData("ThingModel", false, new[] { "ThingModel", "Thing" })]
    [InlineData("IThing", false, new[] { "IThing" })]
    [InlineData("IThingModel", false, new[] { "IThingModel", "IThing" })]

    // isInterface = true
    [InlineData(null, true, new string[0])]
    [InlineData("", true, new string[0])]
    [InlineData("Thing", true, new[] { "Thing" })]
    [InlineData("ThingModel", true, new[] { "ThingModel", "Thing" })]
    [InlineData("IThing", true, new[] { "IThing", "Thing" })]
    [InlineData("IThingModel", true, new[] { "IThingModel", "IThing", "ThingModel", "Thing" })]
    public void CreateListOfNameVariants(string input, bool isInterface, string[] expected)
    {
        var result = DataModelAnalyzer.CreateListOfNameVariants(input, isInterface);
        Equal(expected, result);
    }


    [Theory]
    [InlineData(null, new[] { nameof(DataModelAnalyzerTests) })]
    [InlineData("", new[] { "" })]
    [InlineData("CustomName", new[] { "CustomName" })]
    [InlineData("CustomName,Custom2", new[] { "CustomName", "Custom2" })]
    public void UseSpecifiedNameOrDeriveFromType(string input, string[] expected)
    {
        var result = DataModelAnalyzer.UseSpecifiedNameOrDeriveFromType<DataModelAnalyzerTests>(input);
        Equal(expected, result);
    }
}
