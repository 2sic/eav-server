using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ToSic.Eav.WebApi.Sys.Admin.OData
{
    /// <summary>
    /// Minimal adapter to translate OData AST ($filter, $orderby) into LINQ expressions and apply to IQueryable.
    /// Focuses on core scenarios: property access, constants, comparisons, logical ops, and basic string functions.
    /// </summary>
    public static class FilterOrderByTranslator
    {
        public static IQueryable<T> ApplyTo<T>(IQueryable<T> source, FilterClause filter, OrderByClause orderBy)
        {
            if (filter != null)
            {
                var filterLambda = TranslateFilter<T>(filter);
                source = source.Where((Expression<Func<T, bool>>)filterLambda);
            }

            if (orderBy != null)
            {
                source = ApplyOrderBy(source, orderBy);
            }

            return source;
        }

        public static LambdaExpression TranslateFilter<T>(FilterClause filterClause)
        {
            if (filterClause == null) throw new ArgumentNullException(nameof(filterClause));

            var parameter = Expression.Parameter(typeof(T), filterClause.RangeVariable?.Name ?? "$it");
            var translator = new NodeToLinqTranslator(parameter);
            translator.BindRangeVariable(filterClause.RangeVariable, parameter);

            var body = translator.TranslateNode(filterClause.Expression);
            if (body.Type != typeof(bool))
            {
                // Try to coerce to bool if nullable bool
                if (body.Type == typeof(bool?))
                {
                    body = Expression.Coalesce(body, Expression.Constant(false));
                }
                else
                {
                    throw new NotSupportedException($"Filter expression must be boolean, got {body.Type}.");
                }
            }

            return Expression.Lambda(body, parameter);
        }

        public static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> source, OrderByClause orderByClause)
        {
            if (orderByClause == null) return source;

            var parameter = Expression.Parameter(typeof(T), orderByClause.RangeVariable?.Name ?? "$it");
            var translator = new NodeToLinqTranslator(parameter);
            translator.BindRangeVariable(orderByClause.RangeVariable, parameter);

            IOrderedQueryable<T> ordered = null;
            var current = orderByClause;
            bool first = true;
            while (current != null)
            {
                var keyExpr = translator.TranslateNode(current.Expression);
                // Wrap key selector
                var keyLambda = Expression.Lambda(keyExpr, parameter);

                source = ApplySingleOrdering(source, ref ordered, keyLambda, current.Direction == OrderByDirection.Ascending, first);
                first = false;
                current = current.ThenBy;
            }

            return ordered ?? source;
        }

        private static IQueryable<T> ApplySingleOrdering<T>(IQueryable<T> source, ref IOrderedQueryable<T> ordered, LambdaExpression keySelector, bool ascending, bool first)
        {
            var keyType = keySelector.Body.Type;
            var queryableMethods = typeof(Queryable);
            var methodName = first
                ? (ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending))
                : (ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending));

            var genericMethod = queryableMethods
                .GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                .Single()
                .MakeGenericMethod(typeof(T), keyType);

            var call = Expression.Call(
                null,
                genericMethod,
                (ordered ?? source).Expression,
                Expression.Quote(keySelector));

            ordered = (IOrderedQueryable<T>)((ordered ?? source).Provider.CreateQuery<T>(call));
            return ordered;
        }
    }
}

