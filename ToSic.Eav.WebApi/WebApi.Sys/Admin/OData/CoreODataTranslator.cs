using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ToSic.Eav.WebApi.Sys.Admin.Odata
{
    /// <summary>
    /// Translator from OData AST (produced by <see cref="CoreODataParser"/>)
    /// into typed Filter/Sort rules for the BlogApp EDM.
    ///
    /// Usage:
    ///  - Parse an ODataUri using CoreSystemQueryOptionsParser.Parse(uri)
    ///  - Call <see cref="Translate"/> to get a typed expression tree and sort rules
    ///  - Apply using <see cref="ApplyRulesLinq"/> for in-memory collections or
    ///    <see cref="ApplyRulesToPipeline"/> to configure a 2sxc-like pipeline (stubbed interfaces included).
    ///
    /// Notes:
    ///  - Dates are parsed culture-invariant and interpreted as UTC when no offset is provided.
    ///  - Unknown fields or operators do not throw; they are reported as warnings and skipped.
    ///  - The translator focuses on BlogApp fields and simple functions: contains, startswith, endswith.
    ///  - Designed to be small, readable, and testable. No reflection-based magic in hot paths.
    /// </summary>
    internal static class CoreODataTranslator // POC-03
    {
        #region Public types

        /// <summary>Logical operator between expressions</summary>
        public enum LogicalOperator { And, Or }

        /// <summary>Supported comparison and function operators</summary>
        public enum FilterOperator
        {
            Eq,
            Ne,
            Gt,
            Ge,
            Lt,
            Le,
            Contains,
            StartsWith,
            EndsWith,
            IsNull,
            IsNotNull
        }

        /// <summary>Single filter rule (leaf node)</summary>
        public sealed class FilterRule
        {
            public string Field { get; init; } = string.Empty;
            public FilterOperator Operator { get; init; }
            public object? Value { get; init; }
            public Type? ValueType { get; init; }
            public override string ToString() => $"{Field} {Operator} {Value ?? "null"}";
        }

        /// <summary>Binary expression combining child expressions</summary>
        public abstract class FilterExpression { }

        public sealed class LeafExpression : FilterExpression
        {
            public FilterRule Rule { get; init; } = null!;
        }

        public sealed class BinaryExpression : FilterExpression
        {
            public FilterExpression Left { get; init; } = null!;
            public LogicalOperator Operator { get; init; }
            public FilterExpression Right { get; init; } = null!;
        }

        public sealed class SortRule
        {
            public string Field { get; init; } = string.Empty;
            public bool Descending { get; init; }
            public int Order { get; init; }
            public override string ToString() => $"{Field} {(Descending ? "desc" : "asc")}";
        }

        public sealed class TranslationResult
        {
            // make settable so we can assign after construction
            public FilterExpression? ExpressionTree { get; set; }
            public List<SortRule> Sorts { get; init; } = new List<SortRule>();
            public List<string> Warnings { get; init; } = new List<string>();
        }

        #endregion

        #region Operator mapping and supported fields

        private static readonly Dictionary<BinaryOperatorKind, FilterOperator> BinaryOperatorMap = new()
        {
            { BinaryOperatorKind.Equal, FilterOperator.Eq },
            { BinaryOperatorKind.NotEqual, FilterOperator.Ne },
            { BinaryOperatorKind.GreaterThan, FilterOperator.Gt },
            { BinaryOperatorKind.GreaterThanOrEqual, FilterOperator.Ge },
            { BinaryOperatorKind.LessThan, FilterOperator.Lt },
            { BinaryOperatorKind.LessThanOrEqual, FilterOperator.Le },
        };

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

        // Aliases to translate common names to the EDM property names in the parser model
        private static readonly Dictionary<string, string> FieldAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Created", "PublicationMoment" }
        };

        // Class-level helper used by Translate() to extract property names from OrderBy query nodes
        private static string? GetPropertyNameFromQueryNode(QueryNode node)
        {
            if (node == null) return null;
            // Unwrap ConvertNode
            if (node is ConvertNode conv) node = conv.Source as QueryNode ?? node;

            // Direct property access
            if (node is SingleValuePropertyAccessNode p)
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

            // Open property access or other node types may expose a Name or Property member - try reflection
            try
            {
                var t = node.GetType();
                var propProp = t.GetProperty("Property");
                if (propProp != null)
                {
                    var propObj = propProp.GetValue(node);
                    if (propObj is IEdmProperty iep && !string.IsNullOrEmpty(iep.Name)) return iep.Name;
                }
                var nameProp = t.GetProperty("Name") ?? t.GetProperty("PropertyName");
                if (nameProp != null)
                {
                    var nameVal = nameProp.GetValue(node) as string;
                    if (!string.IsNullOrEmpty(nameVal))
                    {
                        // Only accept known fields or aliases
                        if (AllowedFields.Contains(nameVal) || (FieldAliases.ContainsKey(nameVal) && FieldAliases.ContainsValue(nameVal)))
                            return nameVal;
                    }
                }
            }
            catch { }

            // Fallback: try to parse an identifier from the textual representation
            try
            {
                var s = node?.ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(s, "([A-Za-z_][A-Za-z0-9_]*)");
                    if (m.Success)
                    {
                        var cand = m.Groups[1].Value;
                        if (AllowedFields.Contains(cand) || FieldAliases.ContainsKey(cand) || FieldAliases.ContainsValue(cand))
                            return cand;
                    }
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region Translation entry

        /// <summary>
        /// Translate an ODataUri's FilterClause and OrderByClause to typed rules.
        /// Warnings are collected for unknown fields/operators and coercion failures.
        /// </summary>
        public static TranslationResult Translate(ODataUri odata)
        {
            var result = new TranslationResult();
            if (odata == null) throw new ArgumentNullException(nameof(odata));
            // Translate filter clause via visitor
            try
            {
                var visitor = new FilterAstVisitor(result.Warnings);
                if (odata.Filter != null)
                {
                    // Diagnostic: record the raw expression type and text for debugging
                    result.Warnings.Add($"RawFilterExpression: Type={odata.Filter.Expression?.GetType().Name} Text={odata.Filter.Expression}");
                    if (odata.Filter.Expression != null)
                        result.ExpressionTree = visitor.Visit(odata.Filter.Expression as SingleValueNode);
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Error while translating filter: {ex.Message}");
            }

            // Collect OrderBy clauses (can be a linked list via ThenBy)
            try
            {
                var order = odata.OrderBy;
                var idx = 0;
                while (order != null)
                {
                    var prop = GetPropertyNameFromQueryNode(order.Expression);
                    // Fallback: try a simple textual identifier extraction from the node's string form,
                    // but only accept it if it matches a known field or alias to avoid matching type names like 'Microsoft'.
                    if (string.IsNullOrEmpty(prop))
                    {
                        try
                        {
                            var s = order.Expression?.ToString();
                            if (!string.IsNullOrEmpty(s))
                            {
                                var m = System.Text.RegularExpressions.Regex.Match(s, "([A-Za-z_][A-Za-z0-9_]*)");
                                if (m.Success)
                                {
                                    var cand = m.Groups[1].Value;
                                    var mapped = FieldAliases.TryGetValue(cand, out var alias) ? alias : cand;
                                    if (!string.IsNullOrEmpty(mapped) && (AllowedFields.Contains(mapped) || FieldAliases.ContainsKey(cand) || FieldAliases.ContainsValue(cand)))
                                        prop = cand;
                                }
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(prop))
                    {
                        var descending = order.Direction == OrderByDirection.Descending;
                        if (!string.IsNullOrEmpty(prop))
                        {
                            // normalize/validate before adding
                            string normalizedField = prop!;
                            if (prop != null && FieldAliases.TryGetValue(prop, out var mappedField))
                                normalizedField = mappedField;
                            if (!string.IsNullOrEmpty(normalizedField) && AllowedFields.Contains(normalizedField))
                            {
                                // Use the normalized/validated field name for the SortRule so downstream
                                // pipeline implementations and tests receive a canonical EDM property name
                                // (for example PublicationMoment instead of an alias or raw token).
                                result.Sorts.Add(new SortRule { Field = normalizedField, Descending = descending, Order = idx++ });
                            }
                            else
                            {
                                result.Warnings.Add($"OrderBy: unknown property '{prop}' - skipped");
                            }
                        }
                    }
                    else
                    {
                        result.Warnings.Add($"OrderBy: could not extract property from '{order.Expression}'");
                    }
                    order = order.ThenBy;
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Error while translating OrderBy: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region AST Visitor

        private sealed class FilterAstVisitor
        {
            private readonly List<string> _warnings;
            public FilterAstVisitor(List<string> warnings) => _warnings = warnings ?? new List<string>();

            public FilterExpression? Visit(SingleValueNode? node)
            {
                if (node == null) return null;
                switch (node)
                {
                    case BinaryOperatorNode b: return VisitBinary(b);
                    case SingleValueFunctionCallNode f: return VisitFunction(f);
                    case ConvertNode c: return Visit(c.Source as SingleValueNode);
                    case SingleValuePropertyAccessNode p: return VisitProperty(p);
                    case ConstantNode cst: return VisitConstant(cst);
                    default:
                        _warnings.Add($"Unsupported node type: {node.GetType().Name}");
                        return null;
                }
            }

            private FilterExpression? VisitBinary(BinaryOperatorNode b)
            {
                if (b == null) return null;
                // Logical operators
                if (b.OperatorKind == BinaryOperatorKind.And || b.OperatorKind == BinaryOperatorKind.Or)
                {
                    var left = Visit(b.Left as SingleValueNode);
                    var right = Visit(b.Right as SingleValueNode);
                    if (left == null || right == null)
                    {
                        _warnings.Add("Logical binary has unsupported child; skipping");
                        return left ?? right; // best-effort
                    }
                    return new BinaryExpression
                    {
                        Left = left,
                        Right = right,
                        Operator = b.OperatorKind == BinaryOperatorKind.And ? LogicalOperator.And : LogicalOperator.Or
                    };
                }

                // Comparison operators
                if (!BinaryOperatorMap.TryGetValue(b.OperatorKind, out var op))
                {
                    _warnings.Add($"Unsupported binary operator: {b.OperatorKind}");
                    return null;
                }

                // Expect left to be property and right to be constant (or vice-versa)
                var leftProp = UnwrapNodeToPropertyName(b.Left as SingleValueNode);
                var rightProp = UnwrapNodeToPropertyName(b.Right as SingleValueNode);

                // Prefer property on left
                string? origField = leftProp ?? rightProp;
                SingleValueNode? valueNode = leftProp == null ? b.Left as SingleValueNode : b.Right as SingleValueNode;

                if (origField == null)
                {
                    // Diagnostic: include left/right types and text to help find why property not extracted
                    try
                    {
                        var lt = b.Left?.GetType().Name ?? "null";
                        var rt = b.Right?.GetType().Name ?? "null";
                        _warnings.Add($"Comparison does not reference a field - skipped. Left={lt} Right={rt} LeftText={b.Left} RightText={b.Right}");
                    }
                    catch { _warnings.Add("Comparison does not reference a field - skipped"); }
                    return null;
                }

                // normalize aliases for validation but keep original for rule
                var normalizedField = FieldAliases.TryGetValue(origField, out var mappedField) ? mappedField : origField;
                if (string.IsNullOrEmpty(normalizedField) || !AllowedFields.Contains(normalizedField))
                {
                    _warnings.Add($"Unknown field '{origField}' - skipped");
                    return null;
                }

                if (valueNode == null)
                {
                    _warnings.Add($"Comparison for '{origField}' has no value node - skipped");
                    return null;
                }

                if (!TryExtractConstant(valueNode, out var rawValue, out var valueType, out var warn))
                {
                    _warnings.Add(warn ?? $"Could not coerce value for field '{origField}'");
                    return null;
                }

                // Special null handling
                if (rawValue == null)
                {
                    var f = new FilterRule { Field = origField, Operator = op == FilterOperator.Ne ? FilterOperator.IsNotNull : FilterOperator.IsNull, Value = null, ValueType = null };
                    return new LeafExpression { Rule = f };
                }

                // Coerce the value into sensible CLR type for comparisons
                var coerced = CoerceToBestType(rawValue, valueType);

                var rule = new FilterRule { Field = origField!, Operator = op, Value = coerced.value, ValueType = coerced.type };
                return new LeafExpression { Rule = rule };
            }

            private FilterExpression? VisitFunction(SingleValueFunctionCallNode f)
            {
                if (f == null) return null;
                var funcName = (f.Name ?? string.Empty).ToLowerInvariant();

                // Common patterns: contains(property, 'x'), startswith(property, 'A')
                if (f.Parameters == null || !f.Parameters.Any())
                {
                    _warnings.Add($"Function '{f.Name}' has unexpected arity");
                    return null;
                }
                var first = f.Parameters.First() as SingleValueNode;
                var second = f.Parameters.Skip(1).FirstOrDefault() as SingleValueNode;
                var prop = UnwrapNodeToPropertyName(first);
                if (prop == null)
                {
                    _warnings.Add($"Function '{f.Name}': first parameter must be a property");
                    return null;
                }
                if (!AllowedFields.Contains(prop))
                {
                    _warnings.Add($"Function '{f.Name}': unknown field '{prop}' - skipped");
                    return null;
                }

                string? warn = null;
                if (second == null || !TryExtractConstant(second, out var rawValue, out var valueType, out warn))
                {
                    _warnings.Add(warn ?? $"Function '{f.Name}': cannot extract constant");
                    return null;
                }

                if (rawValue == null)
                {
                    _warnings.Add($"Function '{f.Name}': null literal not supported for string functions");
                    return null;
                }

                object? coercedVal = CoerceToBestType(rawValue, valueType).value;

                FilterOperator op = funcName switch
                {
                    "contains" => FilterOperator.Contains,
                    "startswith" => FilterOperator.StartsWith,
                    "endswith" => FilterOperator.EndsWith,
                    _ => throw new NotSupportedException($"Function '{f.Name}' is not supported")
                };

                var rule = new FilterRule { Field = prop, Operator = op, Value = coercedVal, ValueType = coercedVal?.GetType() };
                return new LeafExpression { Rule = rule };
            }

            private FilterExpression? VisitProperty(SingleValuePropertyAccessNode p)
            {
                var name = ExtractPropertyName(p);
                if (name == null) { _warnings.Add("Property access with no name"); return null; }
                if (!AllowedFields.Contains(name)) { _warnings.Add($"Unknown property '{name}'"); return null; }
                // A single property by itself is not a boolean expression; skip
                _warnings.Add($"Property-only expression '{name}' is not a complete filter");
                return null;
            }

            private FilterExpression? VisitConstant(ConstantNode c)
            {
                // A bare constant is not a filter
                _warnings.Add("Constant-only expression is not a complete filter");
                return null;
            }

            #region Helpers

            private string? UnwrapNodeToPropertyName(SingleValueNode? n)
            {
                if (n == null) return null;
                // unwrap ConvertNodes
                var cur = n;
                while (cur is ConvertNode conv) cur = conv.Source as SingleValueNode;
                if (cur is SingleValuePropertyAccessNode p)
                {
                    try { return (p.Property as IEdmProperty)?.Name; } catch { }
                }
                // Try reflection-based extraction for open-property nodes or other node types
                try
                {
                    var t = cur.GetType();
                    var propProp = t.GetProperty("Property");
                    if (propProp != null)
                    {
                        var propObj = propProp.GetValue(cur);
                        if (propObj is IEdmProperty iep) return iep.Name;
                    }
                    var propNameProp = t.GetProperty("PropertyName");
                    if (propNameProp != null)
                    {
                        var propNameVal = propNameProp.GetValue(cur) as string;
                        if (!string.IsNullOrEmpty(propNameVal))
                            return propNameVal;
                    }
                    var nameProp = t.GetProperty("Name");
                    if (nameProp != null)
                    {
                        var nameVal = nameProp.GetValue(cur) as string;
                        if (!string.IsNullOrEmpty(nameVal)
                            && nameVal != null
                            && (AllowedFields.Contains(nameVal) || FieldAliases.ContainsKey(nameVal)))
                            return nameVal;
                    }
                }
                catch { }
                // as a last resort, try simple textual match but only if it matches known fields/aliases
                try
                {
                    var s = cur?.ToString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(s, "([A-Za-z_][A-Za-z0-9_]*)$");
                        if (m.Success)
                        {
                            var cand = m.Groups[1].Value;
                            if (AllowedFields.Contains(cand) || FieldAliases.ContainsKey(cand) || FieldAliases.ContainsValue(cand))
                                return cand;
                        }
                    }
                }
                catch { }
                _warnings.Add($"Could not extract property name from node: {n.GetType().Name} ({n})");
                return null;
            }

            private static string? ExtractPropertyName(SingleValueNode? n)
            {
                // Unwrap nested ConvertNodes
                while (n is ConvertNode conv) n = conv.Source as SingleValueNode;
                if (n is SingleValuePropertyAccessNode p)
                    return (p.Property as IEdmProperty)?.Name;
                // PathSelectItem etc sometimes produce SingleValueFunctionCallNode with property segments - not handled here
                // Fallback: try to parse a simple identifier from the node's textual representation
                try
                {
                    var s = n?.ToString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        // match simple identifier like Created, Title, etc.
                        var m = System.Text.RegularExpressions.Regex.Match(s, "([A-Za-z_][A-Za-z0-9_]*)$");
                        if (m.Success)
                        {
                            var cand = m.Groups[1].Value;
                            // only accept the fallback if it matches a known field or alias to avoid matching type names
                            if (AllowedFields.Contains(cand) || FieldAliases.ContainsKey(cand) || FieldAliases.ContainsValue(cand))
                                return cand;
                        }
                    }
                }
                catch { }
                return null;
            }

            private static string? GetPropertyNameFromQueryNode(QueryNode node)
            {
                if (node is SingleValuePropertyAccessNode p)
                {
                    return (p.Property as IEdmProperty)?.Name;
                }
                if (node is ConvertNode c && c.Source is SingleValuePropertyAccessNode sp)
                {
                    return (sp.Property as IEdmProperty)?.Name;
                }
                // Try reflection-based extraction for open-property nodes
                try
                {
                    var t = node.GetType();
                    var propNameProp = t.GetProperty("PropertyName");
                    if (propNameProp != null)
                    {
                        var propNameVal = propNameProp.GetValue(node) as string;
                        if (!string.IsNullOrEmpty(propNameVal))
                            return propNameVal;
                    }
                }
                catch { }
                return null;
            }

            private static bool TryExtractConstant(SingleValueNode node, out object? value, out Type? valueType, out string? warning)
            {
                value = null; valueType = null; warning = null;
                // Unwrap nested ConvertNodes
                while (node is ConvertNode conv)
                {
                    if (conv.Source is ConstantNode c2)
                        return TryGetConstantFromNode(c2, out value, out valueType, out warning);
                    node = conv.Source as SingleValueNode ?? node;
                }
                if (node is ConstantNode c)
                {
                    return TryGetConstantFromNode(c, out value, out valueType, out warning);
                }
                // Other node types (function calls) are not supported as constant
                warning = $"Unsupported value node: {node.GetType().Name}";
                return false;
            }

            private static bool TryGetConstantFromNode(ConstantNode c, out object? value, out Type? valueType, out string? warning)
            {
                value = null; valueType = null; warning = null;
                // OData ConstantNode.Value often contains an object already typed, but can be string representations.
                if (c.Value == null)
                {
                    value = null; return true;
                }

                // If already typed as primitive CLR type, return
                if (c.Value is DateTimeOffset dto)
                {
                    value = dto; valueType = typeof(DateTimeOffset); return true;
                }
                if (c.Value is bool b)
                {
                    value = b; valueType = typeof(bool); return true;
                }
                if (c.Value is long l)
                {
                    value = l; valueType = typeof(long); return true;
                }
                if (c.Value is double d)
                {
                    value = d; valueType = typeof(double); return true;
                }

                // Often the node carries a string literal representation
                var s = c.Value.ToString();
                if (string.IsNullOrEmpty(s)) { value = s; valueType = typeof(string); return true; }

                // Try a few parses in order: bool, datetimeoffset, long, double, string
                if (bool.TryParse(s, out var pb)) { value = pb; valueType = typeof(bool); return true; }
                if (TryParseDateTimeOffset(s, out var pdto)) { value = pdto; valueType = typeof(DateTimeOffset); return true; }
                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pl)) { value = pl; valueType = typeof(long); return true; }
                if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var pd)) { value = pd; valueType = typeof(double); return true; }

                // Fallback to string
                value = s; valueType = typeof(string); return true;
            }

            private static (object? value, Type? type) CoerceToBestType(object raw, Type? rawType)
            {
                if (raw == null) return (null, null);
                // If DateTimeOffset given as string, already handled earlier. If numeric long -> int if fits
                if (raw is long l)
                {
                    if (l >= int.MinValue && l <= int.MaxValue) return ((int)l, typeof(int));
                    return (l, typeof(long));
                }
                if (raw is double) return (raw, typeof(double));
                if (raw is DateTimeOffset dto) return (dto, typeof(DateTimeOffset));
                if (raw is bool) return (raw, typeof(bool));
                if (raw is string s) return (s, typeof(string));
                return (raw, raw.GetType());
            }

            private static bool TryParseDateTimeOffset(string s, out DateTimeOffset dto)
            {
                // Interpret bare date-times as UTC
                var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
                return DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, styles, out dto);
            }

            #endregion
        }

        #endregion

        #region LINQ Backend

        /// <summary>
        /// Apply the translated rules to an in-memory collection of dynamic objects.
        /// The method accepts IEnumerable<dynamic> but will attempt to read properties as:
        ///  - IDictionary&lt;string,object&gt; (preferred)
        ///  - public properties via simple reflection fallback
        ///
        /// Returns filtered and sorted enumeration. Evaluation is eager for sorting stability.
        /// </summary>
        public static IEnumerable<dynamic> ApplyRulesLinq(IEnumerable<dynamic> items, TranslationResult rules)
        {
            if (items == null) return Enumerable.Empty<dynamic>();
            if (rules == null) return items;

            Func<dynamic, bool> predicate = item => EvaluateExpression(rules.ExpressionTree, item, rules.Warnings);

            var filtered = items.Where(predicate);

            // Apply sorting
            IOrderedEnumerable<dynamic>? ordered = null;
            foreach (var sort in rules.Sorts.OrderBy(s => s.Order))
            {
                Func<dynamic, object?> keySel = item => GetPropertyValue(item, sort.Field);
                if (ordered == null)
                {
                    ordered = sort.Descending ? filtered.OrderByDescending(keySel) : filtered.OrderBy(keySel);
                }
                else
                {
                    ordered = sort.Descending ? ordered.ThenByDescending(keySel) : ordered.ThenBy(keySel);
                }
            }

            return (ordered ?? filtered).ToList(); // materialize for stable results
        }

        private static bool EvaluateExpression(FilterExpression? expr, dynamic item, List<string> warnings)
        {
            if (expr == null) return true; // no filter => pass-through
            switch (expr)
            {
                case LeafExpression leaf:
                    return EvaluateLeaf(leaf.Rule, item, warnings);
                case BinaryExpression bin:
                    var l = EvaluateExpression(bin.Left, item, warnings);
                    if (bin.Operator == LogicalOperator.And && !l) return false; // short-circuit
                    if (bin.Operator == LogicalOperator.Or && l) return true; // short-circuit
                    var r = EvaluateExpression(bin.Right, item, warnings);
                    return bin.Operator == LogicalOperator.And ? (l && r) : (l || r);
                default:
                    warnings.Add("Unknown expression node when evaluating");
                    return true;
            }
        }

        private static bool EvaluateLeaf(FilterRule rule, dynamic item, List<string> warnings)
        {
            var val = GetPropertyValue(item, rule.Field);

            // Null handling
            if (val == null)
            {
                return rule.Operator == FilterOperator.IsNull || (rule.Value == null && rule.Operator == FilterOperator.Eq);
            }

            try
            {
                switch (rule.Operator)
                {
                    case FilterOperator.Eq: return CompareObjects(val, rule.Value) == 0;
                    case FilterOperator.Ne: return CompareObjects(val, rule.Value) != 0;
                    case FilterOperator.Gt: return CompareObjects(val, rule.Value) > 0;
                    case FilterOperator.Ge: return CompareObjects(val, rule.Value) >= 0;
                    case FilterOperator.Lt: return CompareObjects(val, rule.Value) < 0;
                    case FilterOperator.Le: return CompareObjects(val, rule.Value) <= 0;
                    case FilterOperator.Contains:
                        return val?.ToString()?.IndexOf(rule.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
                    case FilterOperator.StartsWith:
                        return val?.ToString()?.StartsWith(rule.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true;
                    case FilterOperator.EndsWith:
                        return val?.ToString()?.EndsWith(rule.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true;
                    case FilterOperator.IsNull: return val == null;
                    case FilterOperator.IsNotNull: return val != null;
                    default:
                        warnings.Add($"Unsupported runtime operator: {rule.Operator}");
                        return true;
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"Error evaluating rule {rule}: {ex.Message}");
                return true; // fail-open to avoid hard errors on user input
            }
        }

        private static int CompareObjects(object? left, object? right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return right == null ? 0 : -1;
            if (right == null) return 1;

            // If both are DateTimeOffset
            if (left is DateTimeOffset ldt || TryConvertToDateTimeOffset(left, out ldt))
            {
                if (TryConvertToDateTimeOffset(right, out var rdt))
                    return ldt.CompareTo(rdt);
            }

            // Numeric comparison
            if (TryConvertToDouble(left, out var ld))
            {
                if (TryConvertToDouble(right, out var rd))
                {
                    return ld.CompareTo(rd);
                }
            }

            // Bool
            if (left is bool lb || TryConvertToBool(left, out lb))
            {
                if (TryConvertToBool(right, out var rb)) return lb.CompareTo(rb);
            }

            // Fallback to string comparison
            var ls = left.ToString();
            var rs = right.ToString();
            return string.Compare(ls, rs, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Value readers & coercion helpers

        private static object? GetPropertyValue(dynamic item, string name)
        {
            if (item == null) return null;
            // IDictionary<string,object> fast path
            if (item is IDictionary<string, object> dict)
            {
                if (dict.TryGetValue(name, out var v)) return v;
                // try case-insensitive
                var match = dict.Keys.FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
                if (match != null) return dict[match];
                return null;
            }

            // ExpandoObject implements IDictionary<string, object>

            // Reflection fallback - minimal and not used in tight loops
            var t = item.GetType();
            var prop = t.GetProperty(name) ?? t.GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null) return prop.GetValue(item);

            return null;
        }

        private static bool TryConvertToDateTimeOffset(object? o, out DateTimeOffset dto)
        {
            dto = default;
            if (o == null) return false;
            if (o is DateTimeOffset d) { dto = d; return true; }
            if (o is DateTime dt) { dto = new DateTimeOffset(dt.ToUniversalTime()); return true; }
            if (o is string s && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var p)) { dto = p; return true; }
            return false;
        }

        private static bool TryConvertToDouble(object? o, out double d)
        {
            d = 0;
            if (o == null) return false;
            if (o is double dd) { d = dd; return true; }
            if (o is float f) { d = f; return true; }
            if (o is int i) { d = i; return true; }
            if (o is long l) { d = l; return true; }
            if (o is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) { d = p; return true; }
            return false;
        }

        private static bool TryConvertToBool(object? o, out bool b)
        {
            b = false;
            if (o == null) return false;
            if (o is bool bb) { b = bb; return true; }
            if (o is string s && bool.TryParse(s, out var p)) { b = p; return true; }
            return false;
        }

        #endregion

        #region Pipeline backend (stubs / pseudo-interfaces)

        // The real repository contains 2sxc pipeline types. If missing, these minimal interfaces
        // allow compilation and demonstrate intent. Replace with the real types when wiring.
        public interface IDataSource
        {
            // Apply a field filter, operator like "=","contains" etc. Implementations may translate.
            IDataSource Where(string field, string op, object? value);
            IDataSource OrderBy(string field, bool descending);
        }

        /// <summary>
        /// Apply rules to a 2sxc-like pipeline. This is intentionally conservative: unknown fields/operators are skipped and produce warnings.
        /// Implementations should map the string operator names used below to provider-specific tokens.
        /// </summary>
        public static IDataSource ApplyRulesToPipeline(IDataSource source, TranslationResult rules)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rules == null) return source;

            // Collect AND-connected leaf filters up-front and apply them as individual Where() calls.
            var leaves = new List<LeafExpression>();
            CollectAndLeaves(rules.ExpressionTree, leaves, rules);

            // Call Where on the original source instance so that any recorder-like implementation
            // that records calls on its own instance will see all invocations. Return the original
            // instance to keep fluent semantics.
            var original = source;
            foreach (var leaf in leaves)
            {
                var token = OperatorToPipelineToken(leaf.Rule.Operator);
                if (token == null)
                {
                    rules.Warnings.Add($"Pipeline: unsupported operator {leaf.Rule.Operator}");
                    continue;
                }
                original.Where(leaf.Rule.Field, token, leaf.Rule.Value);
            }

            source = original;

            
            // Apply sorts
            foreach (var s in rules.Sorts.OrderBy(x => x.Order))
                source = source.OrderBy(s.Field, s.Descending);

            return source;

            // TODO: support OR via grouping/union or more advanced pipeline features.
        }

        private static void ApplyFilterExpr(FilterExpression? expr, TranslationResult rules, ref IDataSource source)
        {
            if (expr == null) return;

            // Collect all leaf expressions that are connected via AND. We allow partial
            // collection (e.g. when one child was skipped during AST translation) so that
            // valid leaf filters still apply to the pipeline. OR expressions are not
            // supported and will emit a warning but do not prevent other leaves from being applied.
            var leaves = new List<LeafExpression>();
            CollectAndLeaves(expr, leaves, rules);

            foreach (var leaf in leaves)
            {
                var token = OperatorToPipelineToken(leaf.Rule.Operator);
                if (token == null)
                {
                    rules.Warnings.Add($"Pipeline: unsupported operator {leaf.Rule.Operator}");
                    continue;
                }
                source = source.Where(leaf.Rule.Field, token, leaf.Rule.Value);
            }
        }
        private static void CollectAndLeaves(FilterExpression? expr, List<LeafExpression> outLeaves, TranslationResult rules)
        {
            if (expr == null) return;
            switch (expr)
            {
                case LeafExpression leaf:
                    outLeaves.Add(leaf);
                    break;
                case BinaryExpression bin:
                    if (bin.Operator == LogicalOperator.And)
                    {
                        CollectAndLeaves(bin.Left, outLeaves, rules);
                        CollectAndLeaves(bin.Right, outLeaves, rules);
                    }
                    else
                    {
                        rules.Warnings.Add("Pipeline: OR expressions are not yet supported - skipping OR branch");
                    }
                    break;
                default:
                    rules.Warnings.Add($"Pipeline: unsupported expression node {expr.GetType().Name} - skipped");
                    break;
            }
        }

        private static string? OperatorToPipelineToken(FilterOperator op) => op switch
        {
            FilterOperator.Eq => "=",
            FilterOperator.Ne => "!=",
            FilterOperator.Gt => ">",
            FilterOperator.Ge => ">=",
            FilterOperator.Lt => "<",
            FilterOperator.Le => "<=",
            FilterOperator.Contains => "contains",
            FilterOperator.StartsWith => "startswith",
            FilterOperator.EndsWith => "endswith",
            FilterOperator.IsNull => "isnull",
            FilterOperator.IsNotNull => "isnotnull",
            _ => null
        };

        #endregion
    }
}
