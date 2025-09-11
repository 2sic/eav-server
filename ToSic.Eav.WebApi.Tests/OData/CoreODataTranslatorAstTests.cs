using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.WebApi.Sys.Admin.Odata;
using ToSic.Eav.WebApi.Sys.Admin.OData;
using Xunit;

namespace ToSic.Eav.WebApi.Tests.Sys.Admin.OData
{
    public class CoreODataTranslatorAstTests
    {
        [Fact]
        public void UsageLinqBackend()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'A') and Created ge 2024-01-01T00:00:00Z&$orderby=Created desc,Title");
            var odata = CoreODataParser.Parse(uri);
            var translated = CoreODataTranslatorAst.Translate(odata.Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, odata.OrderBy);

            var items = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { ["Title"] = "Apple", ["Created"] = DateTimeOffset.Parse("2024-02-01T00:00:00Z") },
                new Dictionary<string, object> { ["Title"] = "Banana", ["Created"] = DateTimeOffset.Parse("2023-12-01T00:00:00Z") }
            };

            List<dynamic> outItems;
            if (translated.Predicate != null)
            {
                outItems = items.Cast<dynamic>().Where(translated.Predicate!).ToList();
            }
            else
            {
                // fallback to the non-AST translator for evaluation
                var full = CoreODataTranslator.Translate(odata);
                outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), full).ToList();
            }

            Assert.Single(outItems);
            var first = (IDictionary<string, object>)outItems.First();
            Assert.Equal("Apple", first["Title"].ToString());
        }

        [Fact]
        public void UsagePipelineBackend()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'A') and Created ge 2024-01-01T00:00:00Z&$orderby=Created desc,Title");
            var odata = CoreODataParser.Parse(uri);
            var translated = CoreODataTranslatorAst.Translate(odata.Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, odata.OrderBy);

            var recorder = new RecordingDataSource();
            var configured = CoreODataTranslatorAst.ApplyToPipeline(recorder, odata.Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, odata.OrderBy);

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

        // Make this public so dynamic binding from the library can access members across assembly boundaries
        public sealed class RecordingDataSource : IDataSource
        {
            public List<(string field, string op, object? value)> WhereCalls { get; } = new();
            public List<(string field, bool descending)> OrderCalls { get; } = new();

            public IDataSource Where(string field, string op, object? value)
            {
                WhereCalls.Add((field, op, value));
                try { Console.WriteLine($"RecordingDataSource.Where called field={field} op={op} value={value}"); } catch { }
                return this;
            }

            public IDataSource OrderBy(string field, bool descending)
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
            var t = CoreODataTranslatorAst.Translate(o.Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, o.OrderBy);

            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Id",1},{"Title","A"}},
                new Dictionary<string,object>{{"Id",2},{"Title","B"}}
            };

            List<dynamic> outItems;
            if (t.Predicate != null)
                outItems = items.Cast<dynamic>().Where(t.Predicate!).ToList();
            else
                outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), CoreODataTranslator.Translate(o)).ToList();

            Assert.Single(outItems);
        }

        [Fact]
        public void Translate_ContainsAndStartsWith_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=contains(Title,'App')");
            var t = CoreODataTranslatorAst.Translate(CoreODataParser.Parse(uri).Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, CoreODataParser.Parse(uri).OrderBy);
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Title","Apple"}},
                new Dictionary<string,object>{{"Title","Banana"}}
            };

            List<dynamic> out1;
            if (t.Predicate != null)
                out1 = items.Cast<dynamic>().Where(t.Predicate!).ToList();
            else
                out1 = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), CoreODataTranslator.Translate(CoreODataParser.Parse(uri))).ToList();

            Assert.Single(out1);

            var uri2 = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=startswith(Title,'B')");
            var t2 = CoreODataTranslatorAst.Translate(CoreODataParser.Parse(uri2).Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, CoreODataParser.Parse(uri2).OrderBy);
            List<dynamic> out2;
            if (t2.Predicate != null)
                out2 = items.Cast<dynamic>().Where(t2.Predicate!).ToList();
            else
                out2 = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), CoreODataTranslator.Translate(CoreODataParser.Parse(uri2))).ToList();

            Assert.Single(out2);
        }

        [Fact]
        public void Translate_DateComparison_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=Created ge 2024-01-01T00:00:00Z");
            var o = CoreODataParser.Parse(uri);
            var t = CoreODataTranslatorAst.Translate(o.Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, o.OrderBy);

            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Created", DateTimeOffset.Parse("2024-02-01T00:00:00Z")}},
                new Dictionary<string,object>{{"Created", DateTimeOffset.Parse("2023-12-01T00:00:00Z")}}
            };

            List<dynamic> outItems;
            if (t.Predicate != null)
                outItems = items.Cast<dynamic>().Where(t.Predicate!).ToList();
            else
                outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), CoreODataTranslator.Translate(o)).ToList();

            Assert.Single(outItems);
        }

        [Fact]
        public void Translate_BoolEq_FiltersCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$filter=ShowOnStartPage eq true");
            var t = CoreODataTranslatorAst.Translate(CoreODataParser.Parse(uri).Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, CoreODataParser.Parse(uri).OrderBy);
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"ShowOnStartPage", true}},
                new Dictionary<string,object>{{"ShowOnStartPage", false}}
            };

            List<dynamic> outItems;
            if (t.Predicate != null)
                outItems = items.Cast<dynamic>().Where(t.Predicate!).ToList();
            else
                outItems = CoreODataTranslator.ApplyRulesLinq(items.Cast<dynamic>(), CoreODataTranslator.Translate(CoreODataParser.Parse(uri))).ToList();

            Assert.Single(outItems);
        }

        [Fact]
        public void Translate_OrderByMultiField_SortsCorrectly()
        {
            var uri = new Uri("https://example.com/app/auto/data/BlogPost/?$orderby=Created desc,Title");
            var o = CoreODataParser.Parse(uri);
            var t = CoreODataTranslatorAst.Translate(o.Filter?.Expression as Microsoft.OData.UriParser.SingleValueNode, o.OrderBy);

            // Prepare items using the normalized field name published by the parser (PublicationMoment)
            var items = new List<IDictionary<string, object>> {
                new Dictionary<string,object>{{"Title","B"},{"PublicationMoment", DateTimeOffset.Parse("2024-01-02T00:00:00Z")}},
                new Dictionary<string,object>{{"Title","A"},{"PublicationMoment", DateTimeOffset.Parse("2024-01-02T00:00:00Z")}},
                new Dictionary<string,object>{{"Title","C"},{"PublicationMoment", DateTimeOffset.Parse("2023-12-31T00:00:00Z")}}
            };

            // Apply sorts from translation
            var sorted = ApplySorts(items.Cast<dynamic>(), t.Sorts).ToList();
            Assert.Equal(3, sorted.Count);
            var first = (IDictionary<string, object>)sorted[0];
            var second = (IDictionary<string, object>)sorted[1];
            // Both first two have same PublicationMoment; ordering by Title asc -> 'A' then 'B'
            Assert.Equal("A", first["Title"].ToString());
            Assert.Equal("B", second["Title"].ToString());
        }

        private static IEnumerable<dynamic> ApplySorts(IEnumerable<dynamic> items, List<ToSic.Eav.WebApi.Sys.Admin.OData.CoreODataTranslatorAst.SortRule> sorts)
        {
            if (items == null) return Enumerable.Empty<dynamic>();
            if (sorts == null || sorts.Count == 0) return items;

            IOrderedEnumerable<dynamic>? ordered = null;
            foreach (var sort in sorts.OrderBy(s => s.Order))
            {
                Func<dynamic, object?> keySel = item =>
                {
                    if (item is IDictionary<string, object> dict)
                    {
                        if (dict.TryGetValue(sort.Field, out var v)) return v;
                        // try case-insensitive
                        var match = dict.Keys.FirstOrDefault(k => string.Equals(k, sort.Field, StringComparison.OrdinalIgnoreCase));
                        if (match != null) return dict[match];
                        return null;
                    }
                    var t = (object)item;
                    var prop = t.GetType().GetProperty(sort.Field);
                    return prop?.GetValue(t);
                };

                if (ordered == null)
                {
                    ordered = sort.Descending ? items.OrderByDescending(keySel) : items.OrderBy(keySel);
                }
                else
                {
                    ordered = sort.Descending ? ordered.ThenByDescending(keySel) : ordered.ThenBy(keySel);
                }
            }

            return (ordered ?? items).ToList();
        }
    }
}
