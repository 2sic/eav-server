using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using ToSic.Eav.WebApi.Sys.Admin.Query;
using Xunit;

namespace ToSic.Eav.WebApi.Tests.WebApi.Sys.Admin.Query;

public class QueryODataParamsTests
{
    [Fact]
    public void ODataParse_WithValidUri_ReturnsODataUri()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$select=Title,Content");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Path);
        Assert.NotNull(result.SelectAndExpand);
    }

    [Fact]
    public void ODataParse_WithNullUri_ThrowsException()
    {
        // Arrange
        Uri? testUri = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => QueryODataParams.ODataParse(testUri));
    }

    [Fact]
    public void ODataParse_WithUriWithoutServiceRootMarker_ThrowsException()
    {
        // Arrange
        var testUri = new Uri("https://example.com/invalid/path/");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => QueryODataParams.ODataParse(testUri));
    }

    [Fact]
    public void ODataParse_WithSelectQueryOption_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$select=Title,Content,UrlKey");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.NotNull(result.SelectAndExpand);
        var selectedProperties = QueryODataParams.GetSelectedProperties(result.SelectAndExpand);
        Assert.Contains("Title", selectedProperties);
        Assert.Contains("Content", selectedProperties);
        Assert.Contains("UrlKey", selectedProperties);
    }

    [Fact]
    public void ODataParse_WithWildcardSelect_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$select=*");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.NotNull(result.SelectAndExpand);
        var selectedProperties = QueryODataParams.GetSelectedProperties(result.SelectAndExpand);
        Assert.Contains("*", selectedProperties);
    }

    [Fact]
    public void ODataParse_WithFilterQueryOption_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=Id eq 1");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.NotNull(result.Filter);
    }

    [Fact]
    public void ODataParse_WithOrderByQueryOption_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$orderby=Title asc");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.NotNull(result.OrderBy);
    }

    [Fact]
    public void ODataParse_WithTopQueryOption_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$top=10");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.Equal(10, result.Top);
    }

    [Fact]
    public void ODataParse_WithSkipQueryOption_ParsesCorrectly()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$skip=5");

        // Act
        var result = QueryODataParams.ODataParse(testUri);

        // Assert
        Assert.Equal(5, result.Skip);
    }

    [Fact]
    public void GetSelectedProperties_WithNullSelectExpandClause_ReturnsEmptyList()
    {
        // Arrange
        SelectExpandClause? sec = null;

        // Act
#pragma warning disable CS8604 // Possible null reference argument
        var result = QueryODataParams.GetSelectedProperties(sec);
#pragma warning restore CS8604

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetSelectedProperties_WithValidSelectExpandClause_ReturnsPropertyNames()
    {
        // Arrange
        var testUri = new Uri("https://example.com/app/auto/data/BlogPost/?$select=Title,Content");
        var odataUri = QueryODataParams.ODataParse(testUri);

        // Act
        Assert.NotNull(odataUri.SelectAndExpand);
        var result = QueryODataParams.GetSelectedProperties(odataUri.SelectAndExpand);

        // Assert
        Assert.Contains("Title", result);
        Assert.Contains("Content", result);
    }
}