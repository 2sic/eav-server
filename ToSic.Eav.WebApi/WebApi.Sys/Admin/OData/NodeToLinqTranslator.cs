using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ToSic.Eav.WebApi.Sys.Admin.OData
{
    /// <summary>
    /// A pared-down translation of OData QueryNode AST to LINQ Expressions.
    /// Supports: constants, property access, unary not, binary logical and comparisons, and basic string functions.
    /// </summary>
    internal sealed class NodeToLinqTranslator : QueryNodeVisitor<Expression>
    {
        private readonly ParameterExpression _it;
        private readonly Dictionary<RangeVariable, ParameterExpression> _parameters = new();

        public NodeToLinqTranslator(ParameterExpression it)
        {
            _it = it ?? throw new ArgumentNullException(nameof(it));
        }

        public void BindRangeVariable(RangeVariable range, ParameterExpression parameter)
        {
            if (range != null)
            {
                _parameters[range] = parameter ?? _it;
            }
        }

        public Expression TranslateNode(QueryNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return node.Accept(this);
        }

        public override Expression Visit(ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            // Value already typed by ODataUriParser
            var type = node.Value?.GetType() ?? typeof(object);
            return Expression.Constant(node.Value, type);
        }

        public override Expression Visit(ConvertNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            // Keep it simple: rely on LINQ/EF providers for conversions.
            return TranslateNode(node.Source);
        }

        public override Expression Visit(ResourceRangeVariableReferenceNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return _parameters.TryGetValue(node.RangeVariable, out var p) ? p : _it;
        }

        public override Expression Visit(NonResourceRangeVariableReferenceNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return _parameters.TryGetValue(node.RangeVariable, out var p) ? p : _it;
        }

        public override Expression Visit(SingleValuePropertyAccessNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var source = TranslateNode(node.Source);
            var propName = node.Property?.Name ?? throw new NotSupportedException("Missing property in access node.");
            return Expression.PropertyOrField(source, propName);
        }

        public override Expression Visit(SingleComplexNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var source = TranslateNode(node.Source);
            var propName = node.Property?.Name ?? throw new NotSupportedException("Missing property in complex node.");
            return Expression.PropertyOrField(source, propName);
        }

        public override Expression Visit(UnaryOperatorNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var operand = TranslateNode(node.Operand);
            return node.OperatorKind switch
            {
                UnaryOperatorKind.Not => Expression.Not(CoerceToBoolean(operand)),
                UnaryOperatorKind.Negate => Expression.Negate(operand),
                _ => throw new NotSupportedException($"Unary operator {node.OperatorKind} not supported.")
            };
        }

        public override Expression Visit(BinaryOperatorNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var left = TranslateNode(node.Left);
            var right = TranslateNode(node.Right);

            switch (node.OperatorKind)
            {
                case BinaryOperatorKind.And:
                    return Expression.AndAlso(CoerceToBoolean(left), CoerceToBoolean(right));
                case BinaryOperatorKind.Or:
                    return Expression.OrElse(CoerceToBoolean(left), CoerceToBoolean(right));
            }

            // Try to align types for comparison/arithmetic
            (left, right) = PromoteBinaryOperands(left, right);

            return node.OperatorKind switch
            {
                BinaryOperatorKind.Equal => Expression.Equal(left, right),
                BinaryOperatorKind.NotEqual => Expression.NotEqual(left, right),
                BinaryOperatorKind.GreaterThan => Expression.GreaterThan(left, right),
                BinaryOperatorKind.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
                BinaryOperatorKind.LessThan => Expression.LessThan(left, right),
                BinaryOperatorKind.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
                BinaryOperatorKind.Add => Expression.Add(left, right),
                BinaryOperatorKind.Subtract => Expression.Subtract(left, right),
                BinaryOperatorKind.Multiply => Expression.Multiply(left, right),
                BinaryOperatorKind.Divide => Expression.Divide(left, right),
                BinaryOperatorKind.Modulo => Expression.Modulo(left, right),
                _ => throw new NotSupportedException($"Binary operator {node.OperatorKind} not supported.")
            };
        }

        public override Expression Visit(SingleValueFunctionCallNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var name = node.Name?.ToLowerInvariant();
            var args = node.Parameters?.Select(TranslateNode).ToArray() ?? Array.Empty<Expression>();

            // Basic string functions: contains, startswith, endswith
            if (name is "contains" or "startswith" or "endswith")
            {
                if (args.Length != 2)
                    throw new NotSupportedException($"{name} requires 2 arguments.");

                var instance = args[0];
                var value = args[1];

                if (instance.Type != typeof(string))
                    throw new NotSupportedException($"{name} is supported only for string instance.");

                if (value.Type != typeof(string))
                    value = Expression.Convert(value, typeof(string));

                string clrName = name switch
                {
                    "contains" => nameof(string.Contains),
                    "startswith" => nameof(string.StartsWith),
                    "endswith" => nameof(string.EndsWith),
                    _ => name
                };
                var method = typeof(string).GetMethod(clrName, new[] { typeof(string) })
                            ?? throw new InvalidOperationException($"Could not find string.{clrName}(string) method.");
                return Expression.Call(instance, method, value);
            }

            // Case transforms: tolower, toupper
            if (name is "tolower" or "toupper")
            {
                if (args.Length != 1)
                    throw new NotSupportedException($"{name} requires 1 argument.");
                var instance = args[0];
                if (instance.Type != typeof(string)) instance = Expression.Convert(instance, typeof(string));
                var clrName = name == "tolower" ? nameof(string.ToLower) : nameof(string.ToUpper);
                var method = typeof(string).GetMethod(clrName, Type.EmptyTypes)
                             ?? throw new InvalidOperationException($"Could not find string.{clrName}() method.");
                return Expression.Call(instance, method);
            }

            // length(string) -> string.Length
            if (name == "length")
            {
                if (args.Length != 1)
                    throw new NotSupportedException("length requires 1 argument.");
                var instance = args[0];
                if (instance.Type != typeof(string)) instance = Expression.Convert(instance, typeof(string));
                return Expression.Property(instance, nameof(string.Length));
            }

            // substring(string, start[, length])
            if (name == "substring")
            {
                if (args.Length != 2 && args.Length != 3)
                    throw new NotSupportedException("substring requires 2 or 3 arguments.");
                var instance = args[0];
                if (instance.Type != typeof(string)) instance = Expression.Convert(instance, typeof(string));
                var start = args[1].Type == typeof(int) ? args[1] : Expression.Convert(args[1], typeof(int));
                if (args.Length == 2)
                {
                    var m = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int) })
                            ?? throw new InvalidOperationException("string.Substring(int) not found.");
                    return Expression.Call(instance, m, start);
                }
                else
                {
                    var lengthArg = args[2].Type == typeof(int) ? args[2] : Expression.Convert(args[2], typeof(int));
                    var m = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) })
                            ?? throw new InvalidOperationException("string.Substring(int,int) not found.");
                    return Expression.Call(instance, m, start, lengthArg);
                }
            }

            // trim(string) -> string.Trim()
            if (name == "trim")
            {
                if (args.Length != 1)
                    throw new NotSupportedException("trim requires 1 argument.");
                var instance = args[0];
                if (instance.Type != typeof(string)) instance = Expression.Convert(instance, typeof(string));
                var m = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)
                        ?? throw new InvalidOperationException("string.Trim() not found.");
                return Expression.Call(instance, m);
            }

            // indexof(string, value) -> string.IndexOf(string)
            if (name == "indexof")
            {
                if (args.Length != 2)
                    throw new NotSupportedException("indexof requires 2 arguments.");
                var instance = args[0];
                var value = args[1];
                if (instance.Type != typeof(string)) instance = Expression.Convert(instance, typeof(string));
                if (value.Type != typeof(string)) value = Expression.Convert(value, typeof(string));
                var m = typeof(string).GetMethod(nameof(string.IndexOf), new[] { typeof(string) })
                        ?? throw new InvalidOperationException("string.IndexOf(string) not found.");
                return Expression.Call(instance, m, value);
            }

            // concat(string, string) -> string.Concat(string, string)
            if (name == "concat")
            {
                if (args.Length != 2)
                    throw new NotSupportedException("concat requires 2 arguments.");
                var left = args[0];
                var right = args[1];
                if (left.Type != typeof(string)) left = Expression.Convert(left, typeof(string));
                if (right.Type != typeof(string)) right = Expression.Convert(right, typeof(string));
                var m = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) })
                        ?? throw new InvalidOperationException("string.Concat(string,string) not found.");
                return Expression.Call(null, m, left, right);
            }

            throw new NotSupportedException($"Function {node.Name} not supported in this minimal translator.");
        }

        public override Expression Visit(SingleResourceCastNode node)
        {
            // Simplify: ignore type checks/casts for now, return source
            return TranslateNode(node.Source);
        }

        public override Expression Visit(SingleValueCastNode node)
        {
            // Simplify: ignore, rely on provider conversions
            return TranslateNode(node.Source);
        }

        public override Expression Visit(CollectionOpenPropertyAccessNode node) =>
            throw new NotSupportedException("Open properties are not supported in this minimal translator.");

        public override Expression Visit(SingleValueOpenPropertyAccessNode node) =>
            throw new NotSupportedException("Open properties are not supported in this minimal translator.");

        public override Expression Visit(CollectionPropertyAccessNode node) =>
            throw new NotSupportedException("Collection property access is not supported in this minimal translator.");

        public override Expression Visit(CollectionNavigationNode node) =>
            throw new NotSupportedException("Navigation collections are not supported in this minimal translator.");

        public override Expression Visit(SingleNavigationNode node) =>
            throw new NotSupportedException("Navigation singletons are not supported in this minimal translator.");

        public override Expression Visit(AnyNode node) =>
            throw new NotSupportedException("any() is not supported in this minimal translator.");

        public override Expression Visit(AllNode node) =>
            throw new NotSupportedException("all() is not supported in this minimal translator.");

        private static Expression CoerceToBoolean(Expression expr)
        {
            if (expr.Type == typeof(bool)) return expr;
            if (expr.Type == typeof(bool?)) return Expression.Coalesce(expr, Expression.Constant(false));
            throw new NotSupportedException($"Expected boolean expression but found {expr.Type}.");
        }

        private static (Expression Left, Expression Right) PromoteBinaryOperands(Expression left, Expression right)
        {
            if (left.Type == right.Type) return (left, right);

            // If one side is nullable of the other's type, promote the other
            var nnLeft = Nullable.GetUnderlyingType(left.Type);
            var nnRight = Nullable.GetUnderlyingType(right.Type);

            if (nnLeft != null && nnLeft == right.Type)
            {
                return (left, Expression.Convert(right, left.Type));
            }

            if (nnRight != null && nnRight == left.Type)
            {
                return (Expression.Convert(left, right.Type), right);
            }

            // If numeric types, try to promote to a common wider type
            if (IsNumericType(left.Type) && IsNumericType(right.Type))
            {
                var target = GetWiderNumericType(left.Type, right.Type);
                if (left.Type != target) left = Expression.Convert(left, target);
                if (right.Type != target) right = Expression.Convert(right, target);
                return (left, right);
            }

            // Otherwise, if assignable, convert the assignable side
            if (left.Type.IsAssignableFrom(right.Type))
            {
                right = Expression.Convert(right, left.Type);
                return (left, right);
            }
            if (right.Type.IsAssignableFrom(left.Type))
            {
                left = Expression.Convert(left, right.Type);
                return (left, right);
            }

            return (left, right);
        }

        private static bool IsNumericType(Type t)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;
            return t == typeof(byte) || t == typeof(sbyte)
                || t == typeof(short) || t == typeof(ushort)
                || t == typeof(int) || t == typeof(uint)
                || t == typeof(long) || t == typeof(ulong)
                || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }

        private static Type GetWiderNumericType(Type a, Type b)
        {
            var order = new[]
            {
                typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
            };

            int Rank(Type t)
            {
                t = Nullable.GetUnderlyingType(t) ?? t;
                int idx = Array.IndexOf(order, t);
                return idx < 0 ? int.MaxValue : idx;
            }

            var ra = Rank(a);
            var rb = Rank(b);
            return (ra > rb ? a : b); // return the wider of the two given types
        }
    }
}
