using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Tests
{
    public class GoldenTests
    {
        private static ODataQuery Parse(IDictionary<string,string> q) => q.ToQueryTac();

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
            NotNull(parsed.Filter);
            var filterClause = parsed.Filter!;
            NotNull(filterClause.Expression);
            Equal(expected, filterClause.Expression!.ToString());
        }

        [Fact]
        public void OrderBy_ABNF_Item_With_Direction()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$orderby"] = "price desc, name asc" });
            NotNull(parsed.OrderBy);
            var orderBy = parsed.OrderBy!;
            Equal(2, orderBy.Items.Count);
            NotNull(orderBy.Items[0].Expression);
            Equal("price", orderBy.Items[0].Expression!.ToString());
            True(orderBy.Items[0].Descending);
            NotNull(orderBy.Items[1].Expression);
            Equal("name", orderBy.Items[1].Expression!.ToString());
            False(orderBy.Items[1].Descending);
        }

        [Fact]
        public void Compute_ABNF_Item_As_Alias()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$compute"] = "price mul 0.9 as discounted" });
            NotNull(parsed.Compute);
            var compute = parsed.Compute!;
            Single(compute.Items);
            NotNull(compute.Items[0].Expression);
            Equal("(price mul 0.9)", compute.Items[0].Expression!.ToString());
            Equal("discounted", compute.Items[0].Alias);
        }

        [Fact]
        public void Select_Expand_Simple_Lists()
        {
            var parsed = Parse(new Dictionary<string,string> { ["$select"] = "id,name", ["$expand"] = "orders,category" });
            NotNull(parsed.SelectExpand);
            var se = parsed.SelectExpand!;
            NotNull(se.Select);
            NotNull(se.Expand);
            Equal(new[] {"id","name"}, se.Select);
            Equal(new[] {"orders","category"}, se.Expand);
        }

        [Theory]
        [InlineData("\"red shoes\" AND nike")]
        [InlineData("(\"a\" OR b) AND NOT c")]
        public void Search_ABNF_Shapes_Parse(string search)
        {
            var parsed = Parse(new Dictionary<string,string> { ["$search"] = search });
            NotNull(parsed.Search);
            var searchClause = parsed.Search!;
            NotNull(searchClause.Expression);
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
            Equal(10L, parsed.Top);
            Equal(5L, parsed.Skip);
            Equal(-2L, parsed.Index);
            True(parsed.Count.HasValue && parsed.Count.Value);
        }
    }
}
