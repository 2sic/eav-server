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
            var filterClause = parsed.Filter!;
            Assert.NotNull(filterClause.Expression);
            Assert.Equal(expected, filterClause.Expression!.ToString());
        }

        [Fact]
        public void OrderBy_ABNF_Item_With_Direction()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$orderby"] = "price desc, name asc" });
            Assert.NotNull(parsed.OrderBy);
            var orderBy = parsed.OrderBy!;
            Assert.Equal(2, orderBy.Items.Count);
            Assert.NotNull(orderBy.Items[0].Expression);
            Assert.Equal("price", orderBy.Items[0].Expression!.ToString());
            Assert.True(orderBy.Items[0].Descending);
            Assert.NotNull(orderBy.Items[1].Expression);
            Assert.Equal("name", orderBy.Items[1].Expression!.ToString());
            Assert.False(orderBy.Items[1].Descending);
        }

        [Fact]
        public void Compute_ABNF_Item_As_Alias()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$compute"] = "price mul 0.9 as discounted" });
            Assert.NotNull(parsed.Compute);
            var compute = parsed.Compute!;
            Assert.Single(compute.Items);
            Assert.NotNull(compute.Items[0].Expression);
            Assert.Equal("(price mul 0.9)", compute.Items[0].Expression!.ToString());
            Assert.Equal("discounted", compute.Items[0].Alias);
        }

        [Fact]
        public void Select_Expand_Simple_Lists()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$select"] = "id,name", ["$expand"] = "orders,category" });
            Assert.NotNull(parsed.SelectExpand);
            var se = parsed.SelectExpand!;
            Assert.NotNull(se.Select);
            Assert.NotNull(se.Expand);
            Assert.Equal(new[] {"id","name"}, se.Select);
            Assert.Equal(new[] {"orders","category"}, se.Expand);
        }

        [Theory]
        [InlineData("\"red shoes\" AND nike")]
        [InlineData("(\"a\" OR b) AND NOT c")]
        public void Search_ABNF_Shapes_Parse(string search)
        {
            var parsed = Parse(new Dictionary<string,string> { ["$search"] = search });
            Assert.NotNull(parsed.Search);
            var searchClause = parsed.Search!;
            Assert.NotNull(searchClause.Expression);
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
