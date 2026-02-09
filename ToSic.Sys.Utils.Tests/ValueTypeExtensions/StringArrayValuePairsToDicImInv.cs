namespace ToSic.Sys.Utils.Tests.ValueTypeExtensions;

public class StringArrayValuePairsToDicImInv
{
    [Fact]
    public void ValuePairsToDicImInvBasic()
    {
        string[] arr = ["a=abc", "b=bcd", "c=cde"];
        var result = arr.ValuePairsToDicImInv()!;
        Equal(3, result.Count);
        Equal("abc", result["a"]);
        Equal("bcd", result["b"]);
        Equal("cde", result["c"]);
    }

    [Fact]
    public void ValuePairsToDicImInvSkipEmpty()
    {
        string[] arr = ["a=abc", "b=bcd", "c=cde", "d" /* will be skipped */, "e="];
        var result = arr.ValuePairsToDicImInv()!;
        Equal(4, result.Count);
        Equal("abc", result["a"]);
        Equal("bcd", result["b"]);
        Equal("cde", result["c"]);
        Equal("", result["e"]);
    }

    [Fact]
    public void ValuePairsToDicImInvNullArray()
    {
        string[]? arr = null;
        Empty(arr.ValuePairsToDicImInv()!);
    }

    [Fact]
    public void ValuePairsToDicImInvNullArrayPreferNull()
    {
        string[]? arr = null;
        Null(arr.ValuePairsToDicImInv(preferNullToEmpty: true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(null, null)]
    [InlineData("")]
    [InlineData("", null)]
    [InlineData("", "")]
    [InlineData("a", "b")]
    public void ValuePairsToDicImInvEmpty(params string[]? arr) =>
        Empty(arr.ValuePairsToDicImInv()!);

    [Theory]
    [InlineData(null)]
    [InlineData(null, null)]
    [InlineData("")]
    [InlineData("", null)]
    [InlineData("", "")]
    [InlineData("a", "b")]
    public void ValuePairsToDicImInvEmptyPreferNull(params string[]? arr) =>
        Null(arr.ValuePairsToDicImInv(preferNullToEmpty: true)!);
}
