using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ToSic.Eav.WebApi.Sys.Admin.OData
{
    // Public top-level interface so external test assemblies can implement it and be used by the typed ApplyToPipeline overload.
    public interface IDataSource
    {
        IDataSource Where(string field, string op, object? value);
        IDataSource OrderBy(string field, bool descending);
    }

    /// <summary>
    /// Direct translator that operates on OData AST nodes and produces a runtime predicate
    /// and/or applies filters to a pipeline object (dynamic). Non-fatal warnings are collected.
    /// Pipeline supports only AND-combinations of leaves; OR nodes are warned and skipped.
    /// </summary>
    internal static class CoreODataTranslatorAst // POC-04
    {
        public sealed class SortRule
        {
            public string Field { get; init; } = string.Empty;
            public bool Descending { get; init; }
            public int Order { get; init; }
        }

        public sealed class TranslationOutcome
        {
            public Func<dynamic, bool>? Predicate { get; set; }
            public List<SortRule> Sorts { get; } = new List<SortRule>();
            public List<string> Warnings { get; } = new List<string>();
        }

        // Example allowed fields and aliases - adapt as needed
        private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "Created",
            "Title",
            "Author",
            "PublicationMoment",
            "ShowOnStartPage",
            "Image",
            "Id"
        };

        private static readonly Dictionary<string, string> FieldAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Created", "PublicationMoment" }
        };

        private enum FilterOp
        {
            Eq,
            Ne,
            Gt,
            Ge,
            Lt,
            Le
        }

        private static readonly Dictionary<BinaryOperatorKind, FilterOp> BinaryOperatorMap = new()
        {
            { BinaryOperatorKind.Equal, FilterOp.Eq },
            { BinaryOperatorKind.NotEqual, FilterOp.Ne },
            { BinaryOperatorKind.GreaterThan, FilterOp.Gt },
            { BinaryOperatorKind.GreaterThanOrEqual, FilterOp.Ge },
            { BinaryOperatorKind.LessThan, FilterOp.Lt },
            { BinaryOperatorKind.LessThanOrEqual, FilterOp.Le }
        };

        public static TranslationOutcome Translate(SingleValueNode? filterExpression, OrderByClause? orderBy)
        {
            var translationOutcome = new TranslationOutcome();

            if (filterExpression != null)
            {
                var v = new AstToPredicateVisitor(translationOutcome.Warnings);
                translationOutcome.Predicate = v.Visit(filterExpression as SingleValueNode);
            }

            var order = orderBy;
            var i = 0;
            while (order != null)
            {
                var name = GetPropertyNameFromQueryNode(order.Expression);
                if (!string.IsNullOrEmpty(name))
                {
                    translationOutcome.Sorts.Add(new SortRule
                    {
                        Field = name!,
                        Descending = order.Direction == OrderByDirection.Descending,
                        Order = i++
                    });
                }
                else
                {
                    translationOutcome.Warnings.Add($"OrderBy: could not extract property from '{order.Expression}'");
                }

                order = order.ThenBy;
            }

            return translationOutcome;
        }

        // Typed overload that works with IDataSource directly
        public static IDataSource ApplyToPipeline(IDataSource source, SingleValueNode? filterExpression, OrderByClause? orderBy)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (filterExpression != null)
            {
                var warnings = new List<string>();
                var v = new PipelineApplyingVisitorTyped(source, warnings);
                v.Visit(filterExpression as SingleValueNode);
                source = v.Source;
            }

            var order = orderBy;
            while (order != null)
            {
                var name = GetPropertyNameFromQueryNode(order.Expression);
                if (!string.IsNullOrEmpty(name))
                    source = source.OrderBy(name!, order.Direction == OrderByDirection.Descending);

                order = order.ThenBy;
            }

            return source;
        }

        // Fallback dynamic overload; route to typed when possible to avoid runtime binder issues
        public static dynamic ApplyToPipeline(dynamic source, SingleValueNode? filterExpression, OrderByClause? orderBy)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source is IDataSource ds)
                return ApplyToPipeline(ds, filterExpression, orderBy);

            if (filterExpression != null)
            {
                var warnings = new List<string>();
                var v = new PipelineApplyingVisitor(source, warnings);
                v.Visit(filterExpression as SingleValueNode);
                source = v.Source;
            }

            var order = orderBy;
            while (order != null)
            {
                var name = GetPropertyNameFromQueryNode(order.Expression);
                if (!string.IsNullOrEmpty(name))
                    source = source.OrderBy(name!, order.Direction == OrderByDirection.Descending);

                order = order.ThenBy;
            }

            return source;
        }

        // Visitor that converts AST to a runtime Func<dynamic,bool>
        private sealed class AstToPredicateVisitor
        {
            private readonly List<string> _warnings;

            public AstToPredicateVisitor(List<string> warnings) => _warnings = warnings ?? new List<string>();

            public Func<dynamic, bool>? Visit(SingleValueNode? node)
            {
                if (node == null) return null;

                switch (node)
                {
                    case BinaryOperatorNode b:
                        return VisitBinary(b);

                    case SingleValueFunctionCallNode f:
                        return VisitFunction(f);

                    case ConvertNode c:
                        return Visit(c.Source as SingleValueNode);

                    case SingleValuePropertyAccessNode p:
                        return null; // property-only

                    case ConstantNode cst:
                        return null; // constant-only

                    default:
                        _warnings.Add($"Unsupported node: {node.GetType().Name}");
                        return null;
                }
            }

            private Func<dynamic, bool>? VisitBinary(BinaryOperatorNode b)
            {
                if (b.OperatorKind == BinaryOperatorKind.And || b.OperatorKind == BinaryOperatorKind.Or)
                {
                    var l = Visit(b.Left as SingleValueNode);
                    var r = Visit(b.Right as SingleValueNode);

                    if (l == null || r == null)
                    {
                        _warnings.Add("Logical binary: unsupported child");
                        return l ?? r;
                    }

                    return b.OperatorKind == BinaryOperatorKind.And
                        ? new Func<dynamic, bool>(x => l(x) && r(x))
                        : new Func<dynamic, bool>(x => l(x) || r(x));
                }

                if (!BinaryOperatorMap.TryGetValue(b.OperatorKind, out var op))
                {
                    _warnings.Add($"Unsupported operator {b.OperatorKind}");
                    return null;
                }

                var leftProp = UnwrapNodeToPropertyName(b.Left as SingleValueNode);
                var rightProp = UnwrapNodeToPropertyName(b.Right as SingleValueNode);
                var prop = leftProp ?? rightProp;
                var valueNode = leftProp == null ? b.Left as SingleValueNode : b.Right as SingleValueNode;

                if (prop == null || valueNode == null)
                {
                    _warnings.Add("Comparison without field/value");
                    return null;
                }

                var mapped = FieldAliases.TryGetValue(prop, out var alias) ? alias : prop;
                if (!AllowedFields.Contains(mapped))
                {
                    _warnings.Add($"Unknown field '{prop}'");
                    return null;
                }

                if (!TryExtractConstant(valueNode, out var raw, out var type, out var warn))
                {
                    _warnings.Add(warn ?? "Could not extract constant");
                    return null;
                }

                if (raw == null)
                {
                    return op == FilterOp.Ne
                        ? new Func<dynamic, bool>(x => GetPropertyValue(x, prop) != null)
                        : new Func<dynamic, bool>(x => GetPropertyValue(x, prop) == null);
                }

                var coerced = CoerceToBestType(raw, type).value;

                return (dynamic item) =>
                    CompareObjects(GetPropertyValue(item, prop), coerced) switch
                    {
                        0 when op == FilterOp.Eq => true,
                        0 when op == FilterOp.Ge || op == FilterOp.Le => true,
                        var c when c > 0 && (op == FilterOp.Gt || op == FilterOp.Ge) => true,
                        var c when c < 0 && (op == FilterOp.Lt || op == FilterOp.Le) => true,
                        var c when c != 0 && op == FilterOp.Ne => true,
                        _ => false
                    };
            }

            private Func<dynamic, bool>? VisitFunction(SingleValueFunctionCallNode f)
            {
                var name = (f.Name ?? string.Empty).ToLowerInvariant();
                var first = f.Parameters.FirstOrDefault() as SingleValueNode;
                var second = f.Parameters.Skip(1).FirstOrDefault() as SingleValueNode;

                var prop = UnwrapNodeToPropertyName(first);
                if (prop == null)
                {
                    _warnings.Add("Function: first param not property");
                    return null;
                }

                if (!TryExtractConstant(second, out var raw, out var t, out var warn))
                {
                    _warnings.Add(warn ?? "Function: cannot extract constant");
                    return null;
                }

                var s = CoerceToBestType(raw, t).value?.ToString() ?? string.Empty;

                return name switch
                {
                    "contains" => item => (GetPropertyValue(item, prop)?.ToString() ?? string.Empty)
                        .IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0,

                    "startswith" => item => (GetPropertyValue(item, prop)?.ToString() ?? string.Empty)
                        .StartsWith(s, StringComparison.OrdinalIgnoreCase),

                    "endswith" => item => (GetPropertyValue(item, prop)?.ToString() ?? string.Empty)
                        .EndsWith(s, StringComparison.OrdinalIgnoreCase),

                    _ => null
                };
            }
        }

        // Visitor that applies leaves to pipeline immediately (AND-only). OR is warned and skipped.
        private sealed class PipelineApplyingVisitor
        {
            public dynamic Source { get; private set; }

            private readonly List<string> _warnings;

            public PipelineApplyingVisitor(dynamic source, List<string> warnings)
            {
                Source = source;
                _warnings = warnings ?? new List<string>();
            }

            public void Visit(SingleValueNode? node)
            {
                if (node == null) return;

                if (node is BinaryOperatorNode b)
                {
                    if (b.OperatorKind == BinaryOperatorKind.Or)
                    {
                        _warnings.Add("Pipeline: OR not supported");
                        return;
                    }

                    if (b.OperatorKind == BinaryOperatorKind.And)
                    {
                        Visit(b.Left as SingleValueNode);
                        Visit(b.Right as SingleValueNode);
                        return;
                    }

                    ApplyComparison(b);
                    return;
                }

                if (node is SingleValueFunctionCallNode f)
                {
                    ApplyFunction(f);
                    return;
                }

                _warnings.Add($"Unsupported pipeline node: {node.GetType().Name}");
            }

            private void ApplyComparison(BinaryOperatorNode b)
            {
                if (!BinaryOperatorMap.TryGetValue(b.OperatorKind, out var op))
                {
                    _warnings.Add($"Unsupported operator {b.OperatorKind}");
                    return;
                }

                var leftProp = UnwrapNodeToPropertyName(b.Left as SingleValueNode);
                var rightProp = UnwrapNodeToPropertyName(b.Right as SingleValueNode);
                var prop = leftProp ?? rightProp;
                var valueNode = leftProp == null ? b.Left as SingleValueNode : b.Right as SingleValueNode;

                if (prop == null || valueNode == null)
                {
                    _warnings.Add("Pipeline: comparison without field/value");
                    return;
                }

                var mapped = FieldAliases.TryGetValue(prop, out var alias) ? alias : prop;
                if (!AllowedFields.Contains(mapped))
                {
                    _warnings.Add($"Unknown field '{prop}'");
                    return;
                }

                if (!TryExtractConstant(valueNode, out var raw, out var t, out var warn))
                {
                    _warnings.Add(warn ?? "Pipeline: cannot extract constant");
                    return;
                }

                var token = OperatorToPipelineToken(op);
                if (token == null)
                {
                    _warnings.Add("Pipeline: unsupported op");
                    return;
                }

                var value = raw == null ? null : CoerceToBestType(raw, t).value;
                Source = Source.Where(prop, token, value);
            }

            private void ApplyFunction(SingleValueFunctionCallNode f)
            {
                var funcName = (f.Name ?? string.Empty).ToLowerInvariant();
                var first = f.Parameters.FirstOrDefault() as SingleValueNode;
                var second = f.Parameters.Skip(1).FirstOrDefault() as SingleValueNode;

                var prop = UnwrapNodeToPropertyName(first);
                if (prop == null)
                {
                    _warnings.Add("Pipeline function: first param not property");
                    return;
                }

                if (!TryExtractConstant(second, out var raw, out var valueType, out var warn))
                {
                    _warnings.Add(warn ?? $"Function '{f.Name}': cannot extract constant");
                    return;
                }

                if (raw == null)
                {
                    _warnings.Add($"Function '{f.Name}': null literal not supported for string functions");
                    return;
                }

                var coerced = CoerceToBestType(raw, valueType).value;
                var token = funcName switch
                {
                    "contains" => "contains",
                    "startswith" => "startswith",
                    "endswith" => "endswith",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(token))
                {
                    _warnings.Add($"Function '{f.Name}' is not supported");
                    return;
                }

                Source = Source.Where(prop, token, coerced);
            }
        }

        // Typed visitor that uses the IDataSource interface to avoid runtime binder issues
        private sealed class PipelineApplyingVisitorTyped
        {
            public IDataSource Source { get; private set; }
            private readonly List<string> _warnings;

            public PipelineApplyingVisitorTyped(IDataSource source, List<string> warnings)
            {
                Source = source;
                _warnings = warnings ?? new List<string>();
            }

            public void Visit(SingleValueNode? node)
            {
                if (node == null) return;

                if (node is BinaryOperatorNode b)
                {
                    if (b.OperatorKind == BinaryOperatorKind.Or)
                    {
                        _warnings.Add("Pipeline: OR not supported");
                        return;
                    }

                    if (b.OperatorKind == BinaryOperatorKind.And)
                    {
                        Visit(b.Left as SingleValueNode);
                        Visit(b.Right as SingleValueNode);
                        return;
                    }

                    ApplyComparison(b);
                    return;
                }

                if (node is SingleValueFunctionCallNode f)
                {
                    ApplyFunction(f);
                    return;
                }

                _warnings.Add($"Unsupported pipeline node: {node.GetType().Name}");
            }

            private void ApplyComparison(BinaryOperatorNode b)
            {
                if (!BinaryOperatorMap.TryGetValue(b.OperatorKind, out var op))
                {
                    _warnings.Add($"Unsupported operator {b.OperatorKind}");
                    return;
                }

                var leftProp = UnwrapNodeToPropertyName(b.Left as SingleValueNode);
                var rightProp = UnwrapNodeToPropertyName(b.Right as SingleValueNode);
                var prop = leftProp ?? rightProp;
                var valueNode = leftProp == null ? b.Left as SingleValueNode : b.Right as SingleValueNode;

                if (prop == null || valueNode == null)
                {
                    _warnings.Add("Pipeline: comparison without field/value");
                    return;
                }

                var mapped = FieldAliases.TryGetValue(prop, out var alias) ? alias : prop;
                if (!AllowedFields.Contains(mapped))
                {
                    _warnings.Add($"Unknown field '{prop}'");
                    return;
                }

                if (!TryExtractConstant(valueNode, out var raw, out var t, out var warn))
                {
                    _warnings.Add(warn ?? "Pipeline: cannot extract constant");
                    return;
                }

                var token = OperatorToPipelineToken(op);
                if (token == null)
                {
                    _warnings.Add("Pipeline: unsupported op");
                    return;
                }

                var value = raw == null ? null : CoerceToBestType(raw, t).value;
                Source = Source.Where(prop, token, value);
            }

            private void ApplyFunction(SingleValueFunctionCallNode f)
            {
                var funcName = (f.Name ?? string.Empty).ToLowerInvariant();
                var first = f.Parameters.FirstOrDefault() as SingleValueNode;
                var second = f.Parameters.Skip(1).FirstOrDefault() as SingleValueNode;

                var prop = UnwrapNodeToPropertyName(first);
                if (prop == null)
                {
                    _warnings.Add("Pipeline function: first param not property");
                    return;
                }

                if (!TryExtractConstant(second, out var raw, out var valueType, out var warn))
                {
                    _warnings.Add(warn ?? $"Function '{f.Name}': cannot extract constant");
                    return;
                }

                if (raw == null)
                {
                    _warnings.Add($"Function '{f.Name}': null literal not supported for string functions");
                    return;
                }

                var coerced = CoerceToBestType(raw, valueType).value;
                var token = funcName switch
                {
                    "contains" => "contains",
                    "startswith" => "startswith",
                    "endswith" => "endswith",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(token))
                {
                    _warnings.Add($"Function '{f.Name}' is not supported");
                    return;
                }

                Source = Source.Where(prop, token, coerced);
            }
        }

        // Helpers
        private static string? GetPropertyNameFromQueryNode(QueryNode node)
            => node is SingleValuePropertyAccessNode p
                ? p.Property?.Name ?? GetPropertyNameViaReflection(p)
                : node is ConvertNode c && c.Source is SingleValuePropertyAccessNode sp
                    ? sp.Property?.Name ?? GetPropertyNameViaReflection(sp)
                    : null;

        private static string? GetPropertyNameViaReflection(SingleValuePropertyAccessNode p)
        {
            try
            {
                var propNameProp = p.GetType().GetProperty("PropertyName");
                if (propNameProp != null)
                {
                    var propNameVal = propNameProp.GetValue(p) as string;
                    if (!string.IsNullOrEmpty(propNameVal)) return propNameVal;
                }
            }
            catch { }
            return null;
        }

        private static string? UnwrapNodeToPropertyName(SingleValueNode? n)
        {
            if (n == null) return null;

            while (n is ConvertNode conv) n = conv.Source as SingleValueNode;

            if (n is SingleValuePropertyAccessNode p)
            {
                var name = (p.Property as IEdmProperty)?.Name;
                if (!string.IsNullOrEmpty(name)) return name;
                // For open properties, try reflection
                try
                {
                    var propNameProp = p.GetType().GetProperty("PropertyName");
                    if (propNameProp != null)
                    {
                        var propNameVal = propNameProp.GetValue(p) as string;
                        if (!string.IsNullOrEmpty(propNameVal)) return propNameVal;
                    }
                }
                catch { }
            }

            try
            {
                var s = n?.ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    var m = Regex.Match(s, "([A-Za-z_][A-Za-z0-9_]*)$");
                    if (m.Success) return m.Groups[1].Value;
                }
            }
            catch
            {
                // swallow formatting/parsing exceptions - return null below
            }

            return null;
        }

        private static bool TryExtractConstant(SingleValueNode? node, out object? value, out Type? type, out string? warning)
        {
            value = null;
            type = null;
            warning = null;

            if (node == null)
            {
                warning = "Node null";
                return false;
            }

            while (node is ConvertNode conv)
            {
                if (conv.Source is ConstantNode c2)
                    return TryGetConstantFromNode(c2, out value, out type, out warning);

                node = conv.Source as SingleValueNode ?? node;
            }

            if (node is ConstantNode c)
                return TryGetConstantFromNode(c, out value, out type, out warning);

            warning = $"Unsupported value node: {node.GetType().Name}";
            return false;
        }

        private static bool TryGetConstantFromNode(ConstantNode c, out object? value, out Type? type, out string? warning)
        {
            value = null;
            type = null;
            warning = null;

            switch (c.Value)
            {
                case null:
                    value = null;
                    return true;

                case DateTimeOffset dto:
                    value = dto;
                    type = typeof(DateTimeOffset);
                    return true;

                case bool b:
                    value = b;
                    type = typeof(bool);
                    return true;

                case long l:
                    value = l;
                    type = typeof(long);
                    return true;

                case double d:
                    value = d;
                    type = typeof(double);
                    return true;
            }

            var s = c.Value.ToString();
            if (string.IsNullOrEmpty(s))
            {
                value = s;
                type = typeof(string);
                return true;
            }

            if (bool.TryParse(s, out var pb))
            {
                value = pb;
                type = typeof(bool);
                return true;
            }

            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var pdto))
            {
                value = pdto;
                type = typeof(DateTimeOffset);
                return true;
            }

            if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pl))
            {
                value = pl;
                type = typeof(long);
                return true;
            }

            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out var pd))
            {
                value = pd;
                type = typeof(double);
                return true;
            }

            value = s;
            type = typeof(string);
            return true;
        }

        private static (object? value, Type? type) CoerceToBestType(object? raw, Type? rawType)
            => raw switch
            {
                null => (null, null),
                long l when l >= int.MinValue && l <= int.MaxValue => ((object? value, Type? type))((int)l, typeof(int)),
                long l => ((object? value, Type? type))(l, typeof(long)),
                double => (raw, typeof(double)),
                DateTimeOffset dto => ((object? value, Type? type))(dto, typeof(DateTimeOffset)),
                bool => (raw, typeof(bool)),
                string s => ((object? value, Type? type))(s, typeof(string)),
                _ => (raw, raw.GetType()),
            };

        private static int CompareObjects(object? left, object? right)
        {
            if (ReferenceEquals(left, right))
                return 0;

            if (left == null)
                return right == null ? 0 : -1;

            if (right == null)
                return 1;

            if (left is DateTimeOffset ldt || TryConvertToDateTimeOffset(left, out ldt))
                if (TryConvertToDateTimeOffset(right, out var rdt))
                    return ldt.CompareTo(rdt);

            if (TryConvertToDouble(left, out var ld))
                if (TryConvertToDouble(right, out var rd))
                    return ld.CompareTo(rd);

            if (left is bool lb || TryConvertToBool(left, out lb))
                if (TryConvertToBool(right, out var rb))
                    return lb.CompareTo(rb);

            return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryConvertToDateTimeOffset(object? o, out DateTimeOffset dto)
        {
            dto = default;
            switch (o)
            {
                case null:
                    return false;

                case DateTimeOffset d:
                    dto = d;
                    return true;

                case DateTime dt:
                    dto = new DateTimeOffset(dt.ToUniversalTime());
                    return true;

                case string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var p):
                    dto = p;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryConvertToDouble(object? o, out double d)
        {
            d = 0;
            switch (o)
            {
                case null:
                    return false;

                case double dd:
                    d = dd;
                    return true;

                case float f:
                    d = f;
                    return true;

                case int i:
                    d = i;
                    return true;

                case long l:
                    d = l;
                    return true;

                case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var p):
                    d = p;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryConvertToBool(object? o, out bool b)
        {
            b = false;
            switch (o)
            {
                case null:
                    return false;

                case bool bb:
                    b = bb;
                    return true;

                case string s when bool.TryParse(s, out var p):
                    b = p;
                    return true;

                default:
                    return false;
            }
        }

        private static string? OperatorToPipelineToken(FilterOp op)
            => op switch
            {
                FilterOp.Eq => "=",
                FilterOp.Ne => "!=",
                FilterOp.Gt => ">",
                FilterOp.Ge => ">=",
                FilterOp.Lt => "<",
                FilterOp.Le => "<=",
                _ => null
            };

        private static object? GetPropertyValue(dynamic item, string propertyName)
        {
            if (item == null)
                return null;

            try
            {
                if (item is System.Collections.IDictionary dict && dict.Contains(propertyName))
                    return dict[propertyName];

                var t = (object)item;
                var prop = t.GetType().GetProperty(propertyName);

                if (prop != null)
                    return prop.GetValue(t);
            }
            catch
            {
                // ignore reflection failures
            }

            return null;
        }
    }
}
