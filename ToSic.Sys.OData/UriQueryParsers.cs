using System.Globalization;
using ToSic.Sys.OData.Ast;
using ToSic.Sys.OData.Internal;
using ToSic.Sys.Utils;

namespace ToSic.Sys.OData;

public static class UriQueryParser
{
    // Entry: dictionary with keys like $filter, $orderby, etc.
    public static ODataQuery Parse(IDictionary<string, string> queryOptions)
    {

        if (queryOptions == null! /* paranoid */)
            return new();

        var hasSelect = queryOptions.TryGetValue(ODataConstants.SelectParamName, out var selExpSelect);
        var hasExpand = queryOptions.TryGetValue(ODataConstants.ExpandParamName, out var selExpExpand);
        var exp = hasSelect || hasExpand
            ? new SelectExpandClause
            {
                Select = hasSelect && selExpSelect != null
                    ? SplitComma(selExpSelect).ToList()
                    : [],
                Expand = hasExpand && selExpExpand != null
                    ? SplitComma(selExpExpand).ToList()
                    : []
            }
            : null;


        var result = new ODataQuery
        {
            Filter = queryOptions.TryGetValue(ODataConstants.FilterParamName, out var filter)
                ? new() { Expression = new FilterExpressionParser(filter).ParseExpression() }
                : null,

            OrderBy = queryOptions.TryGetValue(ODataConstants.OrderByParamName, out var orderby)
                ? ParseOrderBy(orderby)
                : null,

            SelectExpand = exp,

            Search = queryOptions.TryGetValue(ODataConstants.SearchParamName, out var search)
                ? new() { Expression = new SearchExpressionParser(search).ParseSearch() }
                : null,

            Compute = queryOptions.TryGetValue(ODataConstants.ComputeParamName, out var compute)
                ? ParseCompute(compute)
                : null,

            Top = queryOptions.TryGetValue(ODataConstants.TopParamName, out var top) && long.TryParse(top, NumberStyles.Integer, CultureInfo.InvariantCulture, out var topVal)
                ? topVal
                : null,

            Skip = queryOptions.TryGetValue(ODataConstants.SkipParamName, out var skip) && long.TryParse(skip, NumberStyles.Integer, CultureInfo.InvariantCulture, out var skipVal)
                ? skipVal
                : null,

            Index = queryOptions.TryGetValue(ODataConstants.IndexParamName, out var index) && long.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idxVal)
                ? idxVal
                : null,

            Count = queryOptions.TryGetValue(ODataConstants.CountParamName, out var count)
                ? string.Equals(count, "true", StringComparison.OrdinalIgnoreCase)
                    ? true
                    : string.Equals(count, "false", StringComparison.OrdinalIgnoreCase)
                        ? false
                        : null
                : null,

            SkipToken = queryOptions.TryGetValue(ODataConstants.SkipTokenParamName, out var sk)
                ? sk
                : null,

            DeltaToken = queryOptions.TryGetValue(ODataConstants.DeltaTokenParamName, out var dt)
                ? dt
                : null,
        };

        #region Old Code, non Functional

        //if (queryOptions.TryGetValue(ODataConstants.FilterParamName, out var filter)) 
        //    result.Filter = new() { Expression = new FilterExpressionParser(filter).ParseExpression() };

        //if (queryOptions.TryGetValue(ODataConstants.OrderByParamName, out var orderby))
        //    result.OrderBy = ParseOrderBy(orderby);        

        //var hasSelect = queryOptions.TryGetValue(ODataConstants.SelectParamName, out var select);
        //var hasExpand = queryOptions.TryGetValue(ODataConstants.ExpandParamName, out var expand);
        //if (hasSelect || hasExpand)
        //{
        //    result.SelectExpand = new SelectExpandClause();
        //    if (hasSelect && select != null)
        //        foreach (var s in SplitComma(select))
        //            result.SelectExpand.Select.Add(s);
        //    if (hasExpand && expand != null)
        //        foreach (var e in SplitComma(expand))
        //            result.SelectExpand.Expand.Add(e);
        //}

        //if (queryOptions.TryGetValue(ODataConstants.SearchParamName, out var search))
        //    result.Search = new() { Expression = new SearchExpressionParser(search).ParseSearch() };

        //if (queryOptions.TryGetValue(ODataConstants.ComputeParamName, out var compute))
        //    result.Compute = ParseCompute(compute);

        //if (queryOptions.TryGetValue(ODataConstants.TopParamName, out var top) && long.TryParse(top, NumberStyles.Integer, CultureInfo.InvariantCulture, out var topVal))
        //    result.Top = topVal;

        //if (queryOptions.TryGetValue(ODataConstants.SkipParamName, out var skip) && long.TryParse(skip, NumberStyles.Integer, CultureInfo.InvariantCulture, out var skipVal))
        //    result.Skip = skipVal;

        //if (queryOptions.TryGetValue(ODataConstants.IndexParamName, out var index) && long.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idxVal))
        //    result.Index = idxVal;

        //if (queryOptions.TryGetValue(ODataConstants.CountParamName, out var count))
        //{
        //    if (string.Equals(count, "true", StringComparison.OrdinalIgnoreCase))
        //        result.Count = true;
        //    else if (string.Equals(count, "false", StringComparison.OrdinalIgnoreCase))
        //        result.Count = false;
        //}

        //if (queryOptions.TryGetValue(ODataConstants.SkipTokenParamName, out var sk))
        //    result.SkipToken = sk;

        //if (queryOptions.TryGetValue(ODataConstants.DeltaTokenParamName, out var dt))
        //    result.DeltaToken = dt;

        #endregion

        return result;
    }

    public static ODataQuery Parse(ODataOptions oDataOptions)
    {

        if (oDataOptions.IsEmpty())
            return new();

        var hasSelect = oDataOptions.Select.Any();
        var hasExpand = oDataOptions.Expand.HasValue();
        var expand = hasSelect || hasExpand
            ? new SelectExpandClause
            {
                Select = hasSelect ? [..oDataOptions.Select] : [],
                Expand = hasExpand ? SplitComma(oDataOptions.Expand!).ToList() : []
            }
            : null;

        var result = new ODataQuery
        {
            Filter = oDataOptions.Filter.HasValue()
                ? new() { Expression = new FilterExpressionParser(oDataOptions.Filter).ParseExpression() }
                : null,
            OrderBy = oDataOptions.OrderBy.HasValue()
                ? ParseOrderBy(oDataOptions.OrderBy)
                : null,
            SelectExpand = expand,
            Search = oDataOptions.Search.HasValue()
                ? new() { Expression = new SearchExpressionParser(oDataOptions.Search).ParseSearch() }
                : null,
            Compute = oDataOptions.Compute.HasValue()
                ? ParseCompute(oDataOptions.Compute)
                : null,

            Top = oDataOptions.Top,

            Skip = oDataOptions.Skip,

            Index = oDataOptions.Index,

            Count = oDataOptions.Count,

            SkipToken = oDataOptions.SkipToken.HasValue()
                ? oDataOptions.SkipToken
                : null,

            DeltaToken = oDataOptions.DeltaToken,
        };

        #region Old Code non functional

        //if (oDataOptions.Filter.HasValue())
        //{
        //    result.Filter = new() { Expression = new FilterExpressionParser(oDataOptions.Filter).ParseExpression() };
        //}

        //if (oDataOptions.OrderBy.HasValue())
        //{
        //    result.OrderBy = ParseOrderBy(oDataOptions.OrderBy);
        //}

        //var hasSelect = oDataOptions.Select.Any();
        //var hasExpand = oDataOptions.Expand.HasValue();
        //if (hasSelect || hasExpand)
        //{
        //    result.SelectExpand = new();
        //    if (hasSelect) 
        //        result.SelectExpand.Select.AddRange(oDataOptions.Select);
        //    if (hasExpand)
        //        foreach (var e in SplitComma(oDataOptions.Expand!))
        //            result.SelectExpand.Expand.Add(e);
        //}

        //if (oDataOptions.Search.HasValue())
        //    result.Search = new() { Expression = new SearchExpressionParser(oDataOptions.Search).ParseSearch() };

        //if (oDataOptions.Compute.HasValue())
        //    result.Compute = ParseCompute(oDataOptions.Compute);

        //if (oDataOptions.Top.HasValue)
        //    result.Top = oDataOptions.Top.Value;

        //if (oDataOptions.Skip.HasValue)
        //    result.Skip = oDataOptions.Skip.Value;

        //if (oDataOptions.Index.HasValue)
        //    result.Index = oDataOptions.Index.Value;

        //if (oDataOptions.Count.HasValue) 
        //    result.Count = oDataOptions.Count.Value;

        //if (oDataOptions.SkipToken.HasValue())
        //    result.SkipToken = oDataOptions.SkipToken;

        //if (oDataOptions.DeltaToken.HasValue())
        //    result.DeltaToken = oDataOptions.DeltaToken;

        #endregion

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
            clause.Items.Add(new() { Expression = expr, Descending = desc });
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
            clause.Items.Add(new() { Expression = expr, Alias = alias });
        }
        return clause;
    }
}