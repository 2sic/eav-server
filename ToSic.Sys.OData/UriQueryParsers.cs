using System.Globalization;
using ToSic.Sys.OData.Ast;
using ToSic.Sys.OData.Internal;
using ToSic.Sys.Utils;

namespace ToSic.Sys.OData;

public static class UriQueryParser
{
    // Entry: dictionary with keys like $filter, $orderby, etc.
    public static Query Parse(IDictionary<string, string> queryOptions)
    {
        var result = new Query();

        if (queryOptions == null)
            return result;

        if (queryOptions.TryGetValue(ODataConstants.FilterParamName, out var filter)) 
            result.Filter = new FilterClause { Expression = new FilterExpressionParser(filter).ParseExpression() };

        if (queryOptions.TryGetValue(ODataConstants.OrderByParamName, out var orderby))
            result.OrderBy = ParseOrderBy(orderby);

        var hasSelect = queryOptions.TryGetValue(ODataConstants.SelectParamName, out var select);
        var hasExpand = queryOptions.TryGetValue(ODataConstants.ExpandParamName, out var expand);
        if (hasSelect || hasExpand)
        {
            result.SelectExpand = new SelectExpandClause();
            if (hasSelect && select != null)
                foreach (var s in SplitComma(select))
                    result.SelectExpand.Select.Add(s);
            if (hasExpand && expand != null)
                foreach (var e in SplitComma(expand))
                    result.SelectExpand.Expand.Add(e);
        }

        if (queryOptions.TryGetValue(ODataConstants.SearchParamName, out var search))
            result.Search = new SearchClause { Expression = new SearchExpressionParser(search).ParseSearch() };

        if (queryOptions.TryGetValue(ODataConstants.ComputeParamName, out var compute))
            result.Compute = ParseCompute(compute);

        if (queryOptions.TryGetValue(ODataConstants.TopParamName, out var top) && long.TryParse(top, NumberStyles.Integer, CultureInfo.InvariantCulture, out var topVal))
            result.Top = topVal;

        if (queryOptions.TryGetValue(ODataConstants.SkipParamName, out var skip) && long.TryParse(skip, NumberStyles.Integer, CultureInfo.InvariantCulture, out var skipVal))
            result.Skip = skipVal;

        if (queryOptions.TryGetValue(ODataConstants.IndexParamName, out var index) && long.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idxVal))
            result.Index = idxVal;

        if (queryOptions.TryGetValue(ODataConstants.CountParamName, out var count))
        {
            if (string.Equals(count, "true", StringComparison.OrdinalIgnoreCase))
                result.Count = true;
            else if (string.Equals(count, "false", StringComparison.OrdinalIgnoreCase))
                result.Count = false;
        }

        if (queryOptions.TryGetValue(ODataConstants.SkipTokenParamName, out var sk))
            result.SkipToken = sk;

        if (queryOptions.TryGetValue(ODataConstants.DeltaTokenParamName, out var dt))
            result.DeltaToken = dt;

        return result;
    }

    public static Query Parse(SystemQueryOptions systemQueryOptions)
    {
        var result = new Query();

        if (!systemQueryOptions.RawAllSystem.Any()) return result;

        if (systemQueryOptions.Filter.HasValue())
        {
            result.Filter = new FilterClause { Expression = new FilterExpressionParser(systemQueryOptions.Filter).ParseExpression() };
        }

        if (systemQueryOptions.OrderBy.HasValue())
        {
            result.OrderBy = ParseOrderBy(systemQueryOptions.OrderBy);
        }

        var hasSelect = systemQueryOptions.Select.Any();
        var hasExpand = systemQueryOptions.Expand.HasValue();
        if (hasSelect || hasExpand)
        {
            result.SelectExpand = new SelectExpandClause();
            if (hasSelect) 
                result.SelectExpand.Select.AddRange(systemQueryOptions.Select);
            if (hasExpand)
                foreach (var e in SplitComma(systemQueryOptions.Expand!))
                    result.SelectExpand.Expand.Add(e);
        }

        if (systemQueryOptions.Search.HasValue())
            result.Search = new SearchClause { Expression = new SearchExpressionParser(systemQueryOptions.Search).ParseSearch() };

        if (systemQueryOptions.Compute.HasValue())
            result.Compute = ParseCompute(systemQueryOptions.Compute);

        if (systemQueryOptions.Top.HasValue)
            result.Top = systemQueryOptions.Top.Value;

        if (systemQueryOptions.Skip.HasValue)
            result.Skip = systemQueryOptions.Skip.Value;

        if (systemQueryOptions.Index.HasValue)
            result.Index = systemQueryOptions.Index.Value;

        if (systemQueryOptions.Count.HasValue) 
            result.Count = systemQueryOptions.Count.Value;

        if (systemQueryOptions.SkipToken.HasValue())
            result.SkipToken = systemQueryOptions.SkipToken;

        if (systemQueryOptions.DeltaToken.HasValue())
            result.DeltaToken = systemQueryOptions.DeltaToken;

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