using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Sys.OData.Ast;
using ToSic.Sys.OData.Internal;

namespace ToSic.Sys.OData;

public static class UriQueryParser
{
    // Entry: dictionary with keys like $filter, $orderby, etc.
    public static Query Parse(IDictionary<string, string> queryOptions)
    {
        var result = new Query();

        if (queryOptions == null) return result;

        if (queryOptions.TryGetValue("$filter", out var filter))
        {
            result.Filter = new FilterClause { Expression = new FilterExpressionParser(filter).ParseExpression() };
        }

        if (queryOptions.TryGetValue("$orderby", out var orderby))
        {
            result.OrderBy = ParseOrderBy(orderby);
        }

        var hasSelect = queryOptions.TryGetValue("$select", out var select);
        var hasExpand = queryOptions.TryGetValue("$expand", out var expand);
        if (hasSelect || hasExpand)
        {
            result.SelectExpand = new SelectExpandClause();
            if (hasSelect && select != null)
            {
                foreach (var s in SplitComma(select))
                    result.SelectExpand.Select.Add(s);
            }
            if (hasExpand && expand != null)
            {
                foreach (var e in SplitComma(expand))
                    result.SelectExpand.Expand.Add(e);
            }
        }

        if (queryOptions.TryGetValue("$search", out var search))
        {
            result.Search = new SearchClause { Expression = new SearchExpressionParser(search).ParseSearch() };
        }

        if (queryOptions.TryGetValue("$compute", out var compute))
        {
            result.Compute = ParseCompute(compute);
        }

        if (queryOptions.TryGetValue("$top", out var top) && long.TryParse(top, NumberStyles.Integer, CultureInfo.InvariantCulture, out var topVal))
            result.Top = topVal;

        if (queryOptions.TryGetValue("$skip", out var skip) && long.TryParse(skip, NumberStyles.Integer, CultureInfo.InvariantCulture, out var skipVal))
            result.Skip = skipVal;

        if (queryOptions.TryGetValue("$index", out var index) && long.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idxVal))
            result.Index = idxVal;

        if (queryOptions.TryGetValue("$count", out var count))
        {
            if (string.Equals(count, "true", StringComparison.OrdinalIgnoreCase)) result.Count = true;
            else if (string.Equals(count, "false", StringComparison.OrdinalIgnoreCase)) result.Count = false;
        }

        if (queryOptions.TryGetValue("$skiptoken", out var sk)) result.SkipToken = sk;
        if (queryOptions.TryGetValue("$deltatoken", out var dt)) result.DeltaToken = dt;

        return result;
    }

    private static IEnumerable<string> SplitComma(string s)
        => (s ?? string.Empty).Split(',').Select(p => p.Trim()).Where(p => p.Length > 0);

    private static OrderByClause ParseOrderBy(string text)
    {
        var clause = new OrderByClause();
        foreach (var part in SplitComma(text))
        {
            var desc = part.TrimEnd().EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
            var asc = part.TrimEnd().EndsWith(" asc", StringComparison.OrdinalIgnoreCase);
            var exprText = desc ? part.Substring(0, part.Length - 5) : asc ? part.Substring(0, part.Length - 4) : part;
            var expr = new FilterExpressionParser(exprText).ParseExpression();
            clause.Items.Add(new OrderByClause.Item { Expression = expr, Descending = desc });
        }
        return clause;
    }

    private static ComputeClause ParseCompute(string text)
    {
        // format: expr as alias, expr2 as alias2
        var clause = new ComputeClause();
        foreach (var part in SplitComma(text))
        {
            var idx = part.LastIndexOf(" as ", StringComparison.OrdinalIgnoreCase);
            if (idx <= 0) continue;
            var exprText = part.Substring(0, idx).Trim();
            var alias = part.Substring(idx + 4).Trim();
            var expr = new FilterExpressionParser(exprText).ParseExpression();
            clause.Items.Add(new ComputeClause.Item { Expression = expr, Alias = alias });
        }
        return clause;
    }
}