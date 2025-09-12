using System;
using System.Collections.Generic;
using ToSic.Sys.OData.Ast;
using Xunit;

namespace ToSic.Sys.OData.Tests
{
    public class GoldenTests
    {
        private static Query Parse(IDictionary<string,string> q) => UriQueryParser.Parse(q);

        [Theory]
        [InlineData("price eq 10", "(price eq 10)")]
        [InlineData("price gt 10 and contains(name,'abc')", "((price gt 10) and contains(name, 'abc'))")]
        [InlineData("(price gt 10 and contains(name,'abc')) or not discontinued", "(((price gt 10) and contains(name, 'abc')) or (not discontinued))")]
        [InlineData("10 add 5 mul 2", "(10 add (5 mul 2))")]
        [InlineData("(10 add 5) mul 2", "((10 add 5) mul 2)")]
        [InlineData("amount divby 2 eq 3", "((amount divby 2) eq 3)")]
        [InlineData("flag has 'SomeEnum'", "(flag has 'SomeEnum')")]
        [InlineData("color in ('red','green')", "(color in ('red', 'green'))")]
        public void Filter_ABNF_Expressions_Render_As_Text(string filter, string expected)
        {
            var parsed = Parse(new Dictionary<string,string> { ["$filter"] = filter });
            Assert.NotNull(parsed.Filter);
            Assert.Equal(expected, parsed.Filter!.Expression.ToString());
        }

        [Fact]
        public void OrderBy_ABNF_Item_With_Direction()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$orderby"] = "price desc, name asc" });
            Assert.NotNull(parsed.OrderBy);
            Assert.Equal(2, parsed.OrderBy!.Items.Count);
            Assert.Equal("price", parsed.OrderBy.Items[0].Expression.ToString());
            Assert.True(parsed.OrderBy.Items[0].Descending);
            Assert.Equal("name", parsed.OrderBy.Items[1].Expression.ToString());
            Assert.False(parsed.OrderBy.Items[1].Descending);
        }

        [Fact]
        public void Compute_ABNF_Item_As_Alias()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$compute"] = "price mul 0.9 as discounted" });
            Assert.NotNull(parsed.Compute);
            Assert.Equal(1, parsed.Compute!.Items.Count);
            Assert.Equal("(price mul 0.9)", parsed.Compute.Items[0].Expression.ToString());
            Assert.Equal("discounted", parsed.Compute.Items[0].Alias);
        }

        [Fact]
        public void Select_Expand_Simple_Lists()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$select"] = "id,name", ["$expand"] = "orders,category" });
            Assert.NotNull(parsed.SelectExpand);
            Assert.Equal(new[] {"id","name"}, parsed.SelectExpand!.Select);
            Assert.Equal(new[] {"orders","category"}, parsed.SelectExpand!.Expand);
        }

        [Theory]
        [InlineData("\"red shoes\" AND nike")]
        [InlineData("(\"a\" OR b) AND NOT c")]
        public void Search_ABNF_Shapes_Parse(string search)
        {
            var parsed = Parse(new Dictionary<string,string> { ["$search"] = search });
            Assert.NotNull(parsed.Search);
            Assert.NotNull(parsed.Search!.Expression);
        }

        [Fact]
        public void Scalars_Top_Skip_Index_Count()
        {
            var parsed = Parse(new Dictionary<string,string> {
                ["$top"] = "10",
                ["$skip"] = "5",
                ["$index"] = "-2",
                ["$count"] = "true"
            });
            Assert.Equal(10L, parsed.Top);
            Assert.Equal(5L, parsed.Skip);
            Assert.Equal(-2L, parsed.Index);
            Assert.True(parsed.Count.HasValue && parsed.Count.Value);
        }
    }
}
