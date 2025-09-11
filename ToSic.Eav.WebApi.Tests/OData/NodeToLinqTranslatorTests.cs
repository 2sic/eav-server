using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;
using ToSic.Eav.WebApi.Sys.Admin.OData;
using Xunit;

namespace ToSic.Eav.WebApi.Tests.Sys.Admin.OData
{
    public class NodeToLinqTranslatorTests
    {
        private class TestEntity
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public DateTime Created { get; set; }
            public bool IsActive { get; set; }
        }

        [Fact]
        public void TranslateNode_ConstantNode_ReturnsConstantExpression()
        {
            var parameter = Expression.Parameter(typeof(TestEntity), "it");
            var translator = new NodeToLinqTranslator(parameter);

            var constantNode = new ConstantNode(42);
            var result = translator.TranslateNode(constantNode);

            Assert.Equal(ExpressionType.Constant, result.NodeType);
            Assert.Equal(42, ((ConstantExpression)result).Value);
        }

        [Fact]
        public void TranslateNode_NullNode_ThrowsArgumentNullException()
        {
            var parameter = Expression.Parameter(typeof(TestEntity), "it");
            var translator = new NodeToLinqTranslator(parameter);

            Assert.Throws<ArgumentNullException>(() => translator.TranslateNode(null!));
        }

        [Fact]
        public void Constructor_NullParameter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeToLinqTranslator(null!));
        }

        // Note: More detailed tests for NodeToLinqTranslator are tested indirectly
        // through FilterOrderByTranslator tests, as NodeToLinqTranslator is internal
        // and constructing proper OData AST nodes requires complex EDM model setup.
    }
}
