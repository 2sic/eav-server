using ToSic.Eav.Data.Sys;

#pragma warning disable xUnit1026
namespace ToSic.Eav.Data.PropertyLookup;

public class PropertyRequestSpecsTests
{
    [Fact]
    public void DimensionsBlankAutoExpand()
    {
        var specs = new PropReqSpecs("TestField");
        Equal([null], specs.Dimensions);
    }

    [Theory]
    [InlineData(new[] { (string)null! }, new[] { (string)null! }, "null, Auto Expand")]
    [InlineData(new[] { (string)null! }, new[] { (string)null! }, "empty, Auto Expand")]
    [InlineData(new[] { "a", null }, new[] { "a" }, "Auto Expand")]
    [InlineData(new[] { "a", "b", null }, new[] { "a", "b" }, "Auto Expand")]
    [InlineData(new[] { "a", "b", null }, new[] { "a", "b", null }, "Keep, don't add second null")]
    [InlineData(new[] { "a", null, "b", null }, new[] { "a", null, "b" }, "weird, shouldn't happen, but ...")]
    public void Dimensions1AutoExpand(string[] expected, string[] input, string displayName)
    {
        var specs = new PropReqSpecs("TestField", input, false);
        Equal(expected, specs.Dimensions);
    }

    [Theory]
    [InlineData(new string[] { }, new string[] { }, "nothing, Auto Expand")]
    [InlineData(new[] { "a" }, new[] { "a" }, "a, no Auto Expand")]
    public void DimensionsNoModUntouched(string[] expected, string[] input, string displayName)
    {
        var specs = new PropReqSpecs("TestField", input, DimsAreFinal: true);
        Equal(expected, specs.Dimensions);
    }


    [Fact]
    public void DimensionsWithNullUntouched()
    {
        var specs = new PropReqSpecs("TestField", ["a", null], true);
        Equal(new[] { "a", null }, specs.Dimensions);
    }
}