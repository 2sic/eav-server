using System.Globalization;
using ToSic.Eav.Services;
using ToSic.Sys.OData.Ast;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Translates OData system query options into an EAV data-source pipeline using the existing ValueFilter / ValueSort components.
/// The result contains both the filtered <see cref="IEntity"/> list and a lightweight projection honouring the select option.
/// </summary>
public sealed class ODataQueryEngine(IDataSourcesService dataSourcesService)
{
    private readonly DataSourceOptionConverter _optionConverter = new();

    /// <summary>
    /// Apply filter and orderby clauses to the provided root data source and return the filtered entities plus a select projection.
    /// Note: skip/top are applied after materialising the stream because there is no dedicated skip/take data-source yet.
    /// </summary>
    public QueryExecutionResult Execute(IDataSource root, Query query)
    {
        if (root == null)
            throw new ArgumentNullException(nameof(root));
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var pipeline = BuildFilteredAndSorted(root, query);
        var entities = pipeline.List?.ToImmutableOpt()
                       ?? ImmutableList<IEntity>.Empty;

        var sequence = entities.AsEnumerable();
        if (query.Skip.HasValue)
        {
            var skip = ClampToInt(query.Skip.Value);
            sequence = sequence.Skip(skip);
        }
        if (query.Top.HasValue)
        {
            var take = ClampToInt(query.Top.Value);
            sequence = sequence.Take(take);
        }

        var materialised = sequence.ToImmutableOpt();

        // WARNING: It appears that this projection-result is never used!
        // Because at least from what I can tell, only the materialized result is used.
        var projection = new ODataSelectForQueryEngineProbablyNotUsed(query.SelectExpand?.Select)
            .ApplySelect(materialised);
        return new(materialised, projection);
    }

    public IDataSource BuildFilteredAndSorted(IDataSource root, Query query)
    {
        var current = ApplyFilter(root, query.Filter?.Expression);
        current = ApplyOrderBy(current, query.OrderBy);
        return current;
    }

    private IDataSource ApplyFilter(IDataSource current, Expr? expression)
    {
        if (expression == null)
            return current;

        var configs = new List<ValueFilterConfig>();
        CollectFilters(expression, configs);
        if (configs.Count == 0)
            return current;

        var filtered = configs.Aggregate(current, CreateValueFilter);
        return filtered;
    }

    private List<ValueFilterConfig> CollectFilters(Expr expression, List<ValueFilterConfig> configs)
    {
        switch (expression)
        {
            // Check and operation
            case BinaryExpr { Op: BinaryOp.And } binary:
                configs = CollectFilters(binary.Left, configs);
                configs = CollectFilters(binary.Right, configs);
                return configs;
            // Check OR operation - currently throwing an exception since it's not supported
            case BinaryExpr { Op: BinaryOp.Or }:
                throw new NotSupportedException("Logical OR in filter expressions is not supported yet.");
            default:
                var filter = TryCreateFilter(expression)
                             ?? throw new NotSupportedException($"Unsupported filter expression: {expression}");
                configs.Add(filter);
                return configs;
        }
    }

    private ValueFilterConfig? TryCreateFilter(Expr expression)
        => expression switch
        {
            BinaryExpr binary => TryCreateFromBinary(binary),
            CallExpr call => TryCreateFromCall(call),
            UnaryExpr { Op: "not" or "Not", Operand: CallExpr call } => TryCreateNegatedCall(call),
            _ => null
        };

    private ValueFilterConfig? TryCreateFromBinary(BinaryExpr binary)
    {
        var op = binary.Op switch
        {
            BinaryOp.Eq => CompareOperators.OpEquals,
            BinaryOp.Ne => CompareOperators.OpNotEquals,
            BinaryOp.Gt => CompareOperators.OpGt,
            BinaryOp.Ge => CompareOperators.OpGtEquals,
            BinaryOp.Lt => CompareOperators.OpLt,
            BinaryOp.Le => CompareOperators.OpLtEquals,
            _ => null
        };
        if (op == null)
            return null;

        if (!TryResolveAttributeAndValue(binary.Left, binary.Right, out var attribute, out var value))
            if (!TryResolveAttributeAndValue(binary.Right, binary.Left, out attribute, out value))
                return null;

        return new(attribute, op, value);
    }

    private static ValueFilterConfig? TryCreateFromCall(CallExpr call)
    {
        if (call.Arguments.Count < 2)
            return null;

        var function = call.Name?.Trim().ToLowerInvariant();
        var attributeExpr = call.Arguments[0];
        var valueExpr = call.Arguments[1];

        var attribute = attributeExpr is IdentifierExpr identifier
            ? identifier.Name
            : null;
        if (attribute == null)
            return null;

        var value = ConvertValue(valueExpr);

        return function switch
        {
            "contains" => new(attribute, CompareOperators.OpContains, value),
            "startswith" => new(attribute, CompareOperators.OpBegins, value),
            _ => null
        };
    }

    private static ValueFilterConfig? TryCreateNegatedCall(CallExpr call)
    {
        if (call.Arguments.Count < 2)
            return null;
        var function = call.Name?.Trim().ToLowerInvariant();
        if (call.Arguments[0] is not IdentifierExpr attribute)
            return null;

        var value = ConvertValue(call.Arguments[1]);
        return function switch
        {
            "contains" => new(attribute.Name, CompareOperators.OpNotContains, value),
            _ => null
        };
    }

    private static bool TryResolveAttributeAndValue(Expr attributeExpr, Expr valueExpr, out string attribute, out string? value)
    {
        if (attributeExpr is IdentifierExpr identifier)
        {
            attribute = identifier.Name;
            value = ConvertValue(valueExpr);
            return true;
        }

        attribute = string.Empty;
        value = null;
        return false;
    }

    private IDataSource CreateValueFilter(IDataSource upstream, ValueFilterConfig config)
    {
        var optionObject = new
        {
            config.Attribute,
            config.Operator,
            Value = config.Value ?? string.Empty,
            config.Languages,
        };

        var options = _optionConverter.Convert(optionObject, throwIfNull: false, throwIfNoMatch: false);
        if (options?.Values == null)
            throw new InvalidOperationException("Failed to convert ValueFilter configuration to data-source options.");

        return dataSourcesService.Create<ValueFilter>(upstream, options);
    }

    private IDataSource ApplyOrderBy(IDataSource upstream, OrderByClause? clause)
    {
        if (clause == null || clause.Items.Count == 0)
            return upstream;

        var attributes = new List<string>();
        var directions = new List<string>();

        foreach (var item in clause.Items)
        {
            var attribute = ResolveIdentifier(item.Expression);
            attributes.Add(attribute);
            directions.Add(item.Descending ? "desc" : "asc");
        }

        var optionObject = new
        {
            Attributes = string.Join(",", attributes),
            Directions = string.Join(",", directions),
        };

        var options = _optionConverter.Convert(optionObject, throwIfNull: false, throwIfNoMatch: false);
        if (options?.Values == null)
            throw new InvalidOperationException("Failed to convert ValueSort configuration to data-source options.");

        return dataSourcesService.Create<ValueSort>(attach: upstream, options: options);
    }

    private static string ResolveIdentifier(Expr? expression)
        => expression switch
        {
            IdentifierExpr identifier => identifier.Name,
            null => throw new NotSupportedException("Cannot resolve identifier from null expression"),
            _ => throw new NotSupportedException($"Cannot resolve identifier from expression type {expression.GetType().Name}")
        };

    private static string? ConvertValue(Expr expression)
        => expression switch
        {
            LiteralExpr literal => FormatLiteral(literal.Value),
            IdentifierExpr identifier => identifier.Name,
            _ => throw new NotSupportedException($"Unsupported value expression type {expression.GetType().Name}")
        };

    private static string? FormatLiteral(object? value)
        => value switch
        {
            null => null,
            bool b => b ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

    private static int ClampToInt(long value)
        => value switch
        {
            < 0 => 0,
            > int.MaxValue => int.MaxValue,
            _ => (int)value
        };

    private sealed record ValueFilterConfig(
        string Attribute,
        string Operator,
        string? Value,
        string? Languages = default
    );
}

/// <summary>
/// TODO: THIS IS UNCLEAR - the "Projection" is never used except for in tests, which seems wrong
/// </summary>
/// <param name="Items"></param>
/// <param name="Projection"></param>
public sealed record QueryExecutionResult(IReadOnlyList<IEntity> Items, IReadOnlyList<IDictionary<string, object?>> Projection);












