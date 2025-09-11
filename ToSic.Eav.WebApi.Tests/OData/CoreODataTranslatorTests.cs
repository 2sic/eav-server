using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.WebApi.Sys.Admin.Odata;
using Xunit;

namespace ToSic.Eav.WebApi.Tests.Sys.Admin.OData
{
    public class CoreODataTranslatorTests
    {
        [Fact]
        public void UsageLinqBackend()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'A') and Created ge 2024-01-01T00:00:00Z&$orderby=Created desc,Title");
            var odata = CoreODataParser.Parse(uri);
            var translated = CoreODataTranslator.Translate(odata);

            var items = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { ["Title"] = "Apple", ["Created"] = DateTimeOffset.Parse("2024-02-01T00:00:00Z") },
                new Dictionary<string, object> { ["Title"] = "Banana", ["Created"] = DateTimeOffset.Parse("2023-12-01T00:00:00Z") }
            };

            var outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), translated).ToList();
            Assert.Single(outItems);
            var first = (IDictionary<string, object>)outItems.First();
            Assert.Equal("Apple", first["Title"].ToString());
        }

        [Fact]
        public void UsagePipelineBackend()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'A') and Created ge 2024-01-01T00:00:00Z&$orderby=Created desc,Title");
            var odata = CoreODataParser.Parse(uri);
            var translated = CoreODataTranslator.Translate(odata);

            var recorder = new RecordingDataSource();
            var configured = CoreODataTranslator.ApplyRulesToPipeline(recorder, translated);

            // configured should be the same instance (fluent API returns source)
            Assert.Same(recorder, configured);

            // Expect two Where calls: contains(Title,'A') and Created >= 2024-01-01
            Assert.True(recorder.WhereCalls.Count >= 2);
            Assert.Contains(recorder.WhereCalls, c => string.Equals(c.field, "Title", StringComparison.OrdinalIgnoreCase) && string.Equals(c.op, "contains", StringComparison.OrdinalIgnoreCase) && string.Equals(c.value?.ToString(), "A", StringComparison.OrdinalIgnoreCase));

            Assert.Contains(recorder.WhereCalls, c => string.Equals(c.field, "Created", StringComparison.OrdinalIgnoreCase) && string.Equals(c.op, ">=", StringComparison.Ordinal));
            Assert.Contains(recorder.WhereCalls, c => string.Equals(c.field, "Created", StringComparison.OrdinalIgnoreCase) && c.value is DateTimeOffset);

            // Expect OrderBy calls for Title asc and (Created desc OR PublicationMoment desc)
            Assert.Contains(recorder.OrderCalls, c => string.Equals(c.field, "Title", StringComparison.OrdinalIgnoreCase) && c.descending == false);
            Assert.Contains(recorder.OrderCalls, c => (string.Equals(c.field, "Created", StringComparison.OrdinalIgnoreCase) || string.Equals(c.field, "PublicationMoment", StringComparison.OrdinalIgnoreCase)) && c.descending == true);

        }

        private sealed class RecordingDataSource : CoreODataTranslator.IDataSource
        {
            public List<(string field, string op, object? value)> WhereCalls { get; } = new();
            public List<(string field, bool descending)> OrderCalls { get; } = new();

            public CoreODataTranslator.IDataSource Where(string field, string op, object? value)
            {
                WhereCalls.Add((field, op, value));
                try { Console.WriteLine($"RecordingDataSource.Where called field={field} op={op} value={value}"); } catch { }
                return this;
            }

            public CoreODataTranslator.IDataSource OrderBy(string field, bool descending)
            {
                OrderCalls.Add((field, descending));
                try { Console.WriteLine($"RecordingDataSource.OrderBy called field={field} descending={descending}"); } catch { }
                return this;
            }
        }

        [Fact]
        public void Translate_Eq_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=Id eq 1");
            var o = CoreODataParser.Parse(uri);
            var t = CoreODataTranslator.Translate(o);

            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Id",1},{"Title","A"}},
                new Dictionary<string,object>{{"Id",2},{"Title","B"}}
            };
            var outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), t).ToList();
            Assert.Single(outItems);
        }

        [Fact]
        public void Translate_ContainsAndStartsWith_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'App')");
            var t = CoreODataTranslator.Translate(CoreODataParser.Parse(uri));
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Title","Apple"}},
                new Dictionary<string,object>{{"Title","Banana"}}
            };
            var out1 = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), t).ToList();
            Assert.Single(out1);

            var uri2 = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=startswith(Title,'B')");
            var t2 = CoreODataTranslator.Translate(CoreODataParser.Parse(uri2));
            var out2 = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), t2).ToList();
            Assert.Single(out2);
        }

        [Fact]
        public void Translate_DateComparison_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=Created ge 2024-01-01T00:00:00Z");
            var t = CoreODataTranslator.Translate(CoreODataParser.Parse(uri));
            // Quick sanity checks on the translated rule to debug coercion
            Assert.NotNull(t);
            if (t.ExpressionTree == null)
                throw new Exception("Translate returned no expression. Warnings: " + string.Join(";", t.Warnings));
            var leaf = t.ExpressionTree as CoreODataTranslator.LeafExpression;
            if (leaf == null) throw new Exception("Translated expression is not a Leaf. Warnings: " + string.Join(";", t.Warnings));
            var rule = leaf.Rule;
            Assert.Equal(CoreODataTranslator.FilterOperator.Ge, rule.Operator);
            Assert.True(rule.Value is DateTimeOffset, $"Translated value has unexpected type {rule.Value?.GetType()}: {rule.Value} Warnings: {string.Join(";", t.Warnings)}");
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Created", DateTimeOffset.Parse("2024-02-01T00:00:00Z")}},
                new Dictionary<string,object>{{"Created", DateTimeOffset.Parse("2023-12-01T00:00:00Z")}}
            };
            var outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), t).ToList();
            Assert.Single(outItems);
        }

        [Fact]
        public void Translate_BoolEq_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=ShowOnStartPage eq true");
            var t = CoreODataTranslator.Translate(CoreODataParser.Parse(uri));
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"ShowOnStartPage", true}},
                new Dictionary<string,object>{{"ShowOnStartPage", false}}
            };
            var outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), t).ToList();
            Assert.Single(outItems);
        }

        [Fact]
        public void Translate_OrderByMultiField_SortsCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$orderby=Created desc,Title");
            var t = CoreODataTranslator.Translate(CoreODataParser.Parse(uri));
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Title","B"},{"Created", DateTimeOffset.Parse("2024-01-02T00:00:00Z")}},
                new Dictionary<string,object>{{"Title","A"},{"Created", DateTimeOffset.Parse("2024-01-02T00:00:00Z")}},
                new Dictionary<string,object>{{"Title","C"},{"Created", DateTimeOffset.Parse("2023-12-31T00:00:00Z")}}
            };
            var outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), t).ToList();
            Assert.Equal(3, outItems.Count);
            var first = (IDictionary<string, object>)outItems[0];
            var second = (IDictionary<string, object>)outItems[1];
            // Both first two have same Created; ordering by Title asc -> 'A' then 'B'
            Assert.Equal("A", first["Title"].ToString());
            Assert.Equal("B", second["Title"].ToString());
        }
    }
}
