using Microsoft.OData.UriParser;
using ToSic.Eav.WebApi.Sys.Admin.Odata;

namespace ToSic.Eav.WebApi.Tests.Sys.Admin.OData;

public class CoreODataParserFilterTests
{
    [Fact]
    public void ODataParse_WithFilterLogicalAnd_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=Id eq 1 and ShowOnStartPage eq true");

        // Act
        var result = CoreODataParser.Parse(testUri);

        // Assert
        Assert.NotNull(result.Filter);
        var expr = result.Filter.Expression;
        Assert.NotNull(expr);
        // Expect a binary operator node with 'And'
        var bin = Assert.IsType<BinaryOperatorNode>(expr);
        Assert.Equal(BinaryOperatorKind.And, bin.OperatorKind);
    }

    [Fact]
    public void ODataParse_WithFilterContainsFunction_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'hello')");

        // Act
        var result = CoreODataParser.Parse(testUri);

        // Assert
        Assert.NotNull(result.Filter);
        var expr = result.Filter.Expression;
        Assert.NotNull(expr);
        // contains(...) is represented as a single-value function call node
        Assert.IsType<SingleValueFunctionCallNode>(expr);
    }

    [Fact]
    public void ODataParse_WithFilterOrGrouping_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=(Id eq 1) or (Id eq 2)");

        // Act
        var result = CoreODataParser.Parse(testUri);

        // Assert
        Assert.NotNull(result.Filter);
        var expr = result.Filter.Expression;
        Assert.NotNull(expr);
        var bin = Assert.IsType<BinaryOperatorNode>(expr);
        Assert.Equal(BinaryOperatorKind.Or, bin.OperatorKind);
    }

    [Fact]
    public void ODataParse_WithFilterDateTimeComparison_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=PublicationMoment gt 2020-01-01T00:00:00Z");

        // Act
        var result = CoreODataParser.Parse(testUri);

        // Assert
        Assert.NotNull(result.Filter);
        var expr = result.Filter.Expression;
        Assert.NotNull(expr);
        var bin = Assert.IsType<BinaryOperatorNode>(expr);
        Assert.Equal(BinaryOperatorKind.GreaterThan, bin.OperatorKind);

        // Right side may be a ConvertNode wrapping a ConstantNode, or a ConstantNode directly
        SingleValueNode right = bin.Right;
        ConstantNode? constant = null;
        if (right is ConvertNode convert && convert.Source is ConstantNode c)
        {
            constant = c;
        }
        else if (right is ConstantNode c2)
        {
            constant = c2;
        }

        Assert.NotNull(constant);
        Assert.NotNull(constant!.Value);
        Assert.Contains("2020", constant.Value.ToString());
    }

    [Fact]
    public void ODataParse_WithFilterNullCheck_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=Image eq null");

        // Act
        var result = CoreODataParser.Parse(testUri);

        // Assert
        Assert.NotNull(result.Filter);
        var expr = result.Filter.Expression;
        Assert.NotNull(expr);
        var bin = Assert.IsType<BinaryOperatorNode>(expr);
        Assert.Equal(BinaryOperatorKind.Equal, bin.OperatorKind);

        // Right side should be a constant with null value (may be wrapped in ConvertNode)
        SingleValueNode right = bin.Right;
        ConstantNode? constant = null;
        if (right is ConvertNode convert && convert.Source is ConstantNode c)
            constant = c;
        else if (right is ConstantNode c2)
            constant = c2;

        Assert.NotNull(constant);
        Assert.Null(constant!.Value);
    }
}
