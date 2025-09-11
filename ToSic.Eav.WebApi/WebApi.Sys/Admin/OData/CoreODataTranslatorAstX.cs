//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text.RegularExpressions;
//using Microsoft.OData;
//using Microsoft.OData.Edm;
//using Microsoft.OData.UriParser;

//namespace ToSic.Eav.WebApi.Sys.Admin.Odata
//{
//    public interface IDataSource
//    {
//        IDataSource Where(string field, string op, object? value);
//        IDataSource OrderBy(string field, bool descending);
//    }

//    internal static class CoreODataTranslatorAstX // POC-04 wip
//    {
//        public sealed class SortRule { public string Field { get; init; } = string.Empty; public bool Descending { get; init; } public int Order { get; init; } }
//        public sealed class TranslationOutcome { public Func<dynamic, bool>? Predicate { get; set; } public List<SortRule> Sorts { get; } = new(); public List<string> Warnings { get; } = new(); }

//        private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase) { "Created", "Title", "Author", "PublicationMoment", "ShowOnStartPage", "Image", "Id" };
//        private static readonly Dictionary<string, string> FieldAliases = new(StringComparer.OrdinalIgnoreCase) { { "Created", "PublicationMoment" } };

//        private enum FilterOp { Eq, Ne, Gt, Ge, Lt, Le }
//        private static readonly Dictionary<BinaryOperatorKind, FilterOp> BinaryOperatorMap = new()
//        {
//            { BinaryOperatorKind.Equal, FilterOp.Eq }, { BinaryOperatorKind.NotEqual, FilterOp.Ne },
//            { BinaryOperatorKind.GreaterThan, FilterOp.Gt }, { BinaryOperatorKind.GreaterThanOrEqual, FilterOp.Ge },
//            { BinaryOperatorKind.LessThan, FilterOp.Lt }, { BinaryOperatorKind.LessThanOrEqual, FilterOp.Le }
//        };

//        public static TranslationOutcome Translate(ODataUri? odata)
//        {
//            var outc = new TranslationOutcome();
//            if (odata?.Filter?.Expression != null)
//            {
//                var v = new AstToPredicateVisitor(outc.Warnings);
//                outc.Predicate = v.Visit(odata.Filter.Expression as SingleValueNode);
//            }
//            var order = odata?.OrderBy; var i = 0;
//            while (order != null)
//            {
//                var name = GetPropertyNameFromQueryNode(order.Expression);
//                if (!string.IsNullOrEmpty(name)) outc.Sorts.Add(new SortRule { Field = name, Descending = order.Direction == OrderByDirection.Descending, Order = i++ });
//                else outc.Warnings.Add($"OrderBy: could not extract property from '{order.Expression}'");
//                order = order.ThenBy;
//            }
//            return outc;
//        }

//                private static bool TryParseDateTimeOffset(string s, out DateTimeOffset dto)
//                {
//                    var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
//                    return DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, styles, out dto);
//                }

//                #endregion
//            }

//            // Visitor that applies leaves to pipeline immediately (AND-only)
//            private sealed class PipelineApplyingVisitor
//            {
//                public IDataSource Source { get; private set; }
//                private readonly List<string> _warnings;
//                public PipelineApplyingVisitor(IDataSource source, List<string> warnings) { Source = source; _warnings = warnings ?? new List<string>(); }

//                public void Visit(SingleValueNode? node)
//                {
//                    if (node == null) return;
//                    switch (node)
//                    {
//                        case BinaryOperatorNode b: VisitBinary(b); break;
//                        case SingleValueFunctionCallNode f: VisitFunction(f); break;
//                        case ConvertNode c: Visit(c.Source as SingleValueNode); break;
//                        case SingleValuePropertyAccessNode p: VisitProperty(p); break;
//                        case ConstantNode cst: VisitConstant(cst); break;
//                        default: _warnings.Add($"Unsupported node type: {node.GetType().Name}"); break;
//                    }
//                }

//                private void VisitBinary(BinaryOperatorNode b)
//                {
//                    if (b == null) return;
//                    if (b.OperatorKind == BinaryOperatorKind.And || b.OperatorKind == BinaryOperatorKind.Or)
//                    {
//                        // For OR, we cannot express simply in a single Where call - warn and skip
//                        if (b.OperatorKind == BinaryOperatorKind.Or)
//                        {
//                            _warnings.Add("Pipeline: OR expressions are not yet supported - skipped");
//                            return;
//                        }
//                        Visit(b.Left as SingleValueNode);
//                        Visit(b.Right as SingleValueNode);
//                        return;
//                    }

//                    if (!BinaryOperatorMap.TryGetValue(b.OperatorKind, out var op))
//                    {
//                        _warnings.Add($"Unsupported binary operator: {b.OperatorKind}");
//                        return;
//                    }

//                    var leftProp = UnwrapNodeToPropertyName(b.Left as SingleValueNode);
//                    var rightProp = UnwrapNodeToPropertyName(b.Right as SingleValueNode);

//                    string? origField = leftProp ?? rightProp;
//                    SingleValueNode? valueNode = leftProp == null ? b.Left as SingleValueNode : b.Right as SingleValueNode;

//                    if (origField == null)
//                    {
//                        _warnings.Add("Comparison does not reference a field - skipped");
//                        return;
//                    }

//                    var normalizedField = FieldAliases.TryGetValue(origField, out var mappedField) ? mappedField : origField;
//                    if (string.IsNullOrEmpty(normalizedField) || !AllowedFields.Contains(normalizedField))
//                    {
//                        _warnings.Add($"Unknown field '{origField}' - skipped");
//                        return;
//                    }

//                    if (valueNode == null)
//                    {
//                        _warnings.Add($"Comparison for '{origField}' has no value node - skipped");
//                        return;
//                    }

//                    if (!TryExtractConstant(valueNode, out var rawValue, out var valueType, out var warn))
//                    {
//                        _warnings.Add(warn ?? $"Could not coerce value for field '{origField}'");
//                        return;
//                    }

//                    var token = OperatorToPipelineToken(op);
//                    if (token == null)
//                    {
//                        _warnings.Add($"Pipeline: unsupported operator {op}");
//                        return;
//                    }

//                    // Null handling: pipeline token semantics may vary; use consistent tokens for isnull/isnotnull
//                    if (rawValue == null)
//                    {
//                        Source = Source.Where(origField, token, null);
//                        return;
//                    }

//                    var coerced = CoerceToBestType(rawValue, valueType).value;
//                    Source = Source.Where(origField, token, coerced);
//                }

//                private void VisitFunction(SingleValueFunctionCallNode f)
//                {
//                    if (f == null) return;
//                    var funcName = (f.Name ?? string.Empty).ToLowerInvariant();
//                    if (f.Parameters == null || !f.Parameters.Any()) { _warnings.Add($"Function '{f.Name}' has unexpected arity"); return; }
//                    var first = f.Parameters.First() as SingleValueNode;
//                    var second = f.Parameters.Skip(1).FirstOrDefault() as SingleValueNode;
//                    var prop = UnwrapNodeToPropertyName(first);
//                    if (prop == null) { _warnings.Add($"Function '{f.Name}': first parameter must be a property"); return; }
//                    if (!AllowedFields.Contains(prop)) { _warnings.Add($"Function '{f.Name}': unknown field '{prop}' - skipped"); return; }

//                    string? warn = null;
//                    if (second == null || !TryExtractConstant(second, out var rawValue, out var valueType, out warn)) { _warnings.Add(warn ?? $"Function '{f.Name}': cannot extract constant"); return; }
//                    if (rawValue == null) { _warnings.Add($"Function '{f.Name}': null literal not supported for string functions"); return; }

//                    object? coercedVal = CoerceToBestType(rawValue, valueType).value;
//                    string token = funcName switch { "contains" => "contains", "startswith" => "startswith", "endswith" => "endswith", _ => string.Empty };
//                    if (string.IsNullOrEmpty(token)) { _warnings.Add($"Function '{f.Name}' is not supported"); return; }
//                    Source = Source.Where(prop, token, coercedVal);
//                }

//                private void VisitProperty(SingleValuePropertyAccessNode p) { _warnings.Add("Property-only node in pipeline - skipped"); }
//                private void VisitConstant(ConstantNode c) { _warnings.Add("Constant-only node in pipeline - skipped"); }

//                #region helpers (copied/adapted)

//                private static string? UnwrapNodeToPropertyName(SingleValueNode? n)
//                {
//                    if (n == null) return null;
//                    var cur = n;
//                    while (cur is ConvertNode conv) cur = conv.Source as SingleValueNode;
//                    if (cur is SingleValuePropertyAccessNode p)
//                    {
//                        try { return (p.Property as IEdmProperty)?.Name; } catch { }
//                    }
//                    try
//                    {
//                        var t = cur.GetType();
//                        var propProp = t.GetProperty("Property");
//                        if (propProp != null)
//                        {
//                            var propObj = propProp.GetValue(cur);
//                            if (propObj is IEdmProperty iep) return iep.Name;
//                        }
//                        var nameProp = t.GetProperty("Name");
//                        if (nameProp != null)
//                        {
//                            var nameVal = nameProp.GetValue(cur) as string;
//                            if (!string.IsNullOrEmpty(nameVal)
//                                && (AllowedFields.Contains(nameVal) || FieldAliases.ContainsKey(nameVal) || FieldAliases.ContainsValue(nameVal)))
//                                return nameVal;
//                        }
//                    }
//                    catch { }
//                    try
//                    {
//                        var s = cur?.ToString();
//                        if (!string.IsNullOrEmpty(s))
//                        {
//                            var m = Regex.Match(s, "([A-Za-z_][A-Za-z0-9_]*)");
//                            if (m.Success)
//                            {
//                                var cand = m.Groups[1].Value;
//                                if (AllowedFields.Contains(cand) || FieldAliases.ContainsKey(cand) || FieldAliases.ContainsValue(cand))
//                                    return cand;
//                            }
//                        }
//                    }
//                    catch { }
//                    return null;
//                }

//                private static bool TryExtractConstant(SingleValueNode node, out object? value, out Type? valueType, out string? warning)
//                {
//                    value = null; valueType = null; warning = null;
//                    while (node is ConvertNode conv)
//                    {
//                        if (conv.Source is ConstantNode c2)
//                            return TryGetConstantFromNode(c2, out value, out valueType, out warning);
//                        node = conv.Source as SingleValueNode ?? node;
//                    }
//                    if (node is ConstantNode c)
//                    {
//                        return TryGetConstantFromNode(c, out value, out valueType, out warning);
//                    }
//                    warning = $"Unsupported value node: {node.GetType().Name}";
//                    return false;
//                }

//                private static bool TryGetConstantFromNode(ConstantNode c, out object? value, out Type? valueType, out string? warning)
//                {
//                    value = null; valueType = null; warning = null;
//                    if (c.Value == null) { value = null; return true; }
//                    if (c.Value is DateTimeOffset dto) { value = dto; valueType = typeof(DateTimeOffset); return true; }
//                    if (c.Value is bool b) { value = b; valueType = typeof(bool); return true; }
//                    if (c.Value is long l) { value = l; valueType = typeof(long); return true; }
//                    if (c.Value is double d) { value = d; valueType = typeof(double); return true; }

//                    var s = c.Value.ToString();
//                    if (string.IsNullOrEmpty(s)) { value = s; valueType = typeof(string); return true; }
//                    if (bool.TryParse(s, out var pb)) { value = pb; valueType = typeof(bool); return true; }
//                    if (TryParseDateTimeOffset(s, out var pdto)) { value = pdto; valueType = typeof(DateTimeOffset); return true; }
//                    if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pl)) { value = pl; valueType = typeof(long); return true; }
//                    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var pd)) { value = pd; valueType = typeof(double); return true; }
//                    value = s; valueType = typeof(string); return true;
//                }

//                private static (object? value, Type? type) CoerceToBestType(object raw, Type? rawType)
//                {
//                    if (raw == null) return (null, null);
//                    if (raw is long l) { if (l >= int.MinValue && l <= int.MaxValue) return ((int)l, typeof(int)); return (l, typeof(long)); }
//                    if (raw is double) return (raw, typeof(double));
//                    if (raw is DateTimeOffset dto) return (dto, typeof(DateTimeOffset));
//                    if (raw is bool) return (raw, typeof(bool));
//                    if (raw is string s) return (s, typeof(string));
//                    return (raw, raw.GetType());
//                }

//                private static bool TryParseDateTimeOffset(string s, out DateTimeOffset dto)
//                {
//                    var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
//                    return DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, styles, out dto);
//                }

//                #endregion
//            }

//            // Visitor that applies leaves to pipeline immediately (AND-only)
//            private sealed class PipelineApplyingVisitor
//            {
//                public IDataSource Source { get; private set; }
//                private readonly List<string> _warnings;
//                public PipelineApplyingVisitor(IDataSource source, List<string> warnings) { Source = source; _warnings = warnings ?? new List<string>(); }

//                public void Visit(SingleValueNode? node)
//                {
//                    if (node == null) return;
//                    switch (node)
//                    {
//                        case BinaryOperatorNode b: VisitBinary(b); break;
//                        case SingleValueFunctionCallNode f: VisitFunction(f); break;
//                        case ConvertNode c: Visit(c.Source as SingleValueNode); break;
//                        case SingleValuePropertyAccessNode p: VisitProperty(p); break;
//                        case ConstantNode cst: VisitConstant(cst); break;
//                        default: _warnings.Add($"Unsupported node type: {node.GetType().Name}"); break;
//                    }
//                }

//                private void VisitBinary(BinaryOperatorNode b)
//                {
//                    if (b == null) return;
//                    if (b.OperatorKind == BinaryOperatorKind.And || b.OperatorKind == BinaryOperatorKind.Or)
//                    {
//                        // For OR, we cannot express simply in a single Where call - warn and skip
//                        if (b.OperatorKind == BinaryOperatorKind.Or)
//                        {
//                            _warnings.Add("Pipeline: OR expressions are not yet supported - skipped");
//                            return;
//                        }
//                        Visit(b.Left as SingleValueNode);
//                        Visit(b.Right as SingleValueNode);
//                        return;
//                    }

//                    if (!BinaryOperatorMap.TryGetValue(b.OperatorKind, out var op))
//                    {
//                        _warnings.Add($"Unsupported binary operator: {b.OperatorKind}");
//                        return;
//                    }

//                    var leftProp = UnwrapNodeToPropertyName(b.Left as SingleValueNode);
//                    var rightProp = UnwrapNodeToPropertyName(b.Right as SingleValueNode);

//                    string? origField = leftProp ?? rightProp;
//                    SingleValueNode? valueNode = leftProp == null ? b.Left as SingleValueNode : b.Right as SingleValueNode;

//                    if (origField == null)
//                    {
//                        _warnings.Add("Comparison does not reference a field - skipped");
//                        return;
//                    }

//                    var normalizedField = FieldAliases.TryGetValue(origField, out var mappedField) ? mappedField : origField;
//                    if (string.IsNullOrEmpty(normalizedField) || !AllowedFields.Contains(normalizedField))
//                    {
//                        _warnings.Add($"Unknown field '{origField}' - skipped");
//                        return;
//                    }

//                    if (valueNode == null)
//                    {
//                        _warnings.Add($"Comparison for '{origField}' has no value node - skipped");
//                        return;
//                    }

//                    if (!TryExtractConstant(valueNode, out var rawValue, out var valueType, out var warn))
//                    {
//                        _warnings.Add(warn ?? $"Could not coerce value for field '{origField}'");
//                        return;
//                    }

//                    var token = OperatorToPipelineToken(op);
//                    if (token == null)
//                    {
//                        _warnings.Add($"Pipeline: unsupported operator {op}");
//                        return;
//                    }

//                    // Null handling: pipeline token semantics may vary; use consistent tokens for isnull/isnotnull
//                    if (rawValue == null)
//                    {
//                        Source = Source.Where(origField, token, null);
//                        return;
//                    }

//                    var coerced = CoerceToBestType(rawValue, valueType).value;
//                    Source = Source.Where(origField, token, coerced);
//                }

//        private static string? GetPropertyNameFromQueryNode(QueryNode node)
//        {
//            if (node is SingleValuePropertyAccessNode p) return (p.Property as IEdmProperty)?.Name;
//            if (node is ConvertNode c && c.Source is SingleValuePropertyAccessNode sp) return (sp.Property as IEdmProperty)?.Name;
//            return null;
//        }

//        private static bool TryExtractConstant(SingleValueNode node, out object? value, out Type? valueType, out string? warning)
//        {
//            // delegate to visitor helper - reuse implementation style by using a lightweight Convert handling and ConstantNode extraction
//            value = null; valueType = null; warning = null;
//            while (node is ConvertNode conv)
//            {
//                if (conv.Source is ConstantNode c2)
//                    return TryGetConstantFromNode(c2, out value, out valueType, out warning);
//                node = conv.Source as SingleValueNode ?? node;
//            }
//            if (node is ConstantNode c)
//            {
//                return TryGetConstantFromNode(c, out value, out valueType, out warning);
//            }
//            warning = $"Unsupported value node: {node.GetType().Name}";
//            return false;
//        }

//        private static bool TryGetConstantFromNode(ConstantNode c, out object? value, out Type? valueType, out string? warning)
//        {
//            value = null; valueType = null; warning = null;
//            if (c.Value == null) { value = null; return true; }
//            if (c.Value is DateTimeOffset dto) { value = dto; valueType = typeof(DateTimeOffset); return true; }
//            if (c.Value is bool b) { value = b; valueType = typeof(bool); return true; }
//            if (c.Value is long l) { value = l; valueType = typeof(long); return true; }
//            if (c.Value is double d) { value = d; valueType = typeof(double); return true; }
//            var s = c.Value.ToString();
//            if (string.IsNullOrEmpty(s)) { value = s; valueType = typeof(string); return true; }
//            if (bool.TryParse(s, out var pb)) { value = pb; valueType = typeof(bool); return true; }
//            if (TryParseDateTimeOffset(s, out var pdto)) { value = pdto; valueType = typeof(DateTimeOffset); return true; }
//            if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pl)) { value = pl; valueType = typeof(long); return true; }
//            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var pd)) { value = pd; valueType = typeof(double); return true; }
//            value = s; valueType = typeof(string); return true;
//        }

//        private static (object? value, Type? type) CoerceToBestType(object raw, Type? rawType)
//        {
//            if (raw == null) return (null, null);
//            if (raw is long l) { if (l >= int.MinValue && l <= int.MaxValue) return ((int)l, typeof(int)); return (l, typeof(long)); }
//            if (raw is double) return (raw, typeof(double));
//            if (raw is DateTimeOffset dto) return (dto, typeof(DateTimeOffset));
//            if (raw is bool) return (raw, typeof(bool));
//            if (raw is string s) return (s, typeof(string));
//            return (raw, raw.GetType());
//        }

//        private static bool TryParseDateTimeOffset(string s, out DateTimeOffset dto)
//        {
//            var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
//            return DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, styles, out dto);
//        }

//        private static object? GetPropertyValue(dynamic item, string name)
//        {
//            if (item == null) return null;
//            if (item is IDictionary<string, object> dict)
//            {
//                if (dict.TryGetValue(name, out var v)) return v;
//                var match = dict.Keys.FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
//                if (match != null) return dict[match];
//                return null;
//            }
//            var t = item.GetType();
//            var prop = t.GetProperty(name) ?? t.GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//            if (prop != null) return prop.GetValue(item);
//            return null;
//        }

//        private static int CompareObjects(object? left, object? right)
//        {
//            if (ReferenceEquals(left, right)) return 0;
//            if (left == null) return right == null ? 0 : -1;
//            if (right == null) return 1;
//            if (left is DateTimeOffset ldt || TryConvertToDateTimeOffset(left, out ldt))
//            {
//                if (TryConvertToDateTimeOffset(right, out var rdt))
//                    return ldt.CompareTo(rdt);
//            }

//            if (TryConvertToDouble(left, out var ld))
//            {
//                if (TryConvertToDouble(right, out var rd))
//                {
//                    return ld.CompareTo(rd);
//                }
//            }

//            if (left is bool lb || TryConvertToBool(left, out lb))
//            {
//                if (TryConvertToBool(right, out var rb)) return lb.CompareTo(rb);
//            }

//            var ls = left.ToString();
//            var rs = right.ToString();
//            return string.Compare(ls, rs, StringComparison.OrdinalIgnoreCase);
//        }

//        private static bool TryConvertToDateTimeOffset(object? o, out DateTimeOffset dto)
//        {
//            dto = default;
//            if (o == null) return false;
//            if (o is DateTimeOffset d) { dto = d; return true; }
//            if (o is DateTime dt) { dto = new DateTimeOffset(dt.ToUniversalTime()); return true; }
//            if (o is string s && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var p)) { dto = p; return true; }
//            return false;
//        }

//        private static bool TryConvertToDouble(object? o, out double d)
//        {
//            d = 0;
//            if (o == null) return false;
//            if (o is double dd) { d = dd; return true; }
//            if (o is float f) { d = f; return true; }
//            if (o is int i) { d = i; return true; }
//            if (o is long l) { d = l; return true; }
//            if (o is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) { d = p; return true; }
//            return false;
//        }

//        private static bool TryConvertToBool(object? o, out bool b)
//        {
//            b = false;
//            if (o == null) return false;
//            if (o is bool bb) { b = bb; return true; }
//            if (o is string s && bool.TryParse(s, out var p)) { b = p; return true; }
//            return false;
//        }

//        private static string? OperatorToPipelineToken(FilterOp op) => op switch
//        {
//            FilterOp.Eq => "=",
//            FilterOp.Ne => "!=",
//            FilterOp.Gt => ">",
//            FilterOp.Ge => ">=",
//            FilterOp.Lt => "<",
//            FilterOp.Le => "<=",
//            _ => null
//        };

//        #endregion

//        #region IDataSource stub (same as original)

//        public interface IDataSource
//        {
//            IDataSource Where(string field, string op, object? value);
//            IDataSource OrderBy(string field, bool descending);
//        }

//        #endregion
//    }
//}
