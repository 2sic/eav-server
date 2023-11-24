using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static System.StringComparison;
using static ToSic.Eav.DataSources.CompareOperators;

namespace ToSic.Eav.DataSources;

[PrivateApi]
public class ValueComparison: HelperBase
{
    internal ValueComparison(Action<string, string> errCallback, ILog parentLog): base(parentLog, "Eav.DsCmMk")
    {
        _errCallback = errCallback ?? throw new ArgumentNullException(nameof(errCallback));
    }

    private readonly Action<string, string> _errCallback;

    public Func<IEntity, bool> GetComparison(ValueTypes type, string fieldName, string operation,
        string[] languages, string expected) => Log.Func(l =>
    {
        operation = operation.ToLowerInvariant();

        // First try to figure out based on known type
        switch (type)
        {
            case ValueTypes.Boolean:
                return (BoolComparison(fieldName, operation, languages, expected), "bool comparison");
            case ValueTypes.DateTime:
                return (DateTimeComparison(fieldName, operation, languages, expected), "datetime comparison");
            case ValueTypes.Number:
                return (NumberComparison(fieldName, operation, languages, expected), "decimal comparison");
            case ValueTypes.Entity:
                l.A("Would apply entity comparison, but this doesn't work");
                _errCallback("Can't apply Value comparison to Relationship",
                    "Can't compare values which contain related entities - use the RelationshipFilter instead.");
                return (null, "error");
            case ValueTypes.Undefined:
            case ValueTypes.Hyperlink:
            case ValueTypes.String:
            case ValueTypes.Empty:
            case ValueTypes.Custom:
            case ValueTypes.Json:
            default:
                return (StringComparison(fieldName, operation, languages, expected), "string comparison");
        }
            
    });


    /// <summary>
    /// Provide all the string comparison functionality as a prepared function
    /// </summary>
    private Func<IEntity, bool> StringComparison(string fieldName, string operation, string[] languages, string expected) => Log.Func(expected, () =>
    {
        Func<object, bool> StringCompareInner()
        {
            switch (operation)
            {
                case OpEquals: return value => value != null && string.Equals(value.ToString(), expected, InvariantCultureIgnoreCase) ;
                case OpExactly: return value => value != null && value.ToString() == expected; // case sensitive: return full equal
                case OpNotEquals: return value => value != null && !string.Equals(value.ToString(), expected, InvariantCultureIgnoreCase) ;
                case OpContains: return value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) > -1;
                case OpNotContains: return value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) == -1;
                case OpBegins: return value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) == 0;
                case OpAll: return value => true;
                default: return null;
            }
        }

        var stringCompare = StringCompareInner();

        if (stringCompare == null)
        {
            _errCallback(ErrorInvalidOperator, $"Bad operator for string compare, can't find comparison '{operation}'");
            return (null as Func<IEntity, bool>, "error");
        }

        return (e => stringCompare(e.GetBestValue(fieldName, languages)), "ok");
    });


    /// <summary>
    /// Provide all bool-compare functionality as a prepared function
    /// </summary>
    private Func<IEntity, bool> BoolComparison(string fieldName, string operation, string[] languages, string expected) => Log.Func(expected, () =>
    {
        var boolFilter = bool.Parse(expected);

        switch (operation)
        {
            case OpEquals:
            case OpExactly: return (e => e.GetBestValue(fieldName, languages) as bool? == boolFilter, "ok");
            case OpNotEquals: return (e => e.GetBestValue(fieldName, languages) as bool? != boolFilter, "ok");
        }

        _errCallback(ErrorInvalidOperator, $"Bad operator for boolean compare, can't find comparison '{operation}'");
        return (null as Func<IEntity, bool>, "error");
    });


    /// <summary>
    /// provide all number-compare functionality as prepared/precompiled functions
    /// </summary>
    private Func<IEntity, bool> NumberComparison(string fieldName, string operation, string[] languages, string expected) => Log.Func(expected, () =>
    {
        var minOrExpected = decimal.MinValue;
        var max = decimal.MaxValue;
        List<decimal> decimals = null;

        #region check for special case "between" with two values to compare
        if (operation == OpBetween || operation == OpNotBetween)
        {
            Log.A("Operator is between or !between");
            var (useBetween, start, end) = BetweenParts(expected);
            if (useBetween)
            {
                decimal.TryParse(start, out minOrExpected);
                decimal.TryParse(end, out max);
            }
            else
                operation = OpEquals;
        }
        #endregion

        #region check for special case "contains" with many values to compare
        if (operation == OpContains || operation == OpNotContains)
        {
            Log.A($"Operator is {OpContains} or {OpNotContains}");
            decimals = new List<decimal>();
            foreach(var num in expected.Split(','))
                if (decimal.TryParse(num, out var dec))
                    decimals.Add(dec);
        }
        #endregion

        // get the value (but only if it hasn't been initialized already)
        if (minOrExpected == decimal.MinValue)
            decimal.TryParse(expected, out minOrExpected);

        var numberCompare = NumberInnerCompare(operation, minOrExpected, max, decimals) ;
        if (numberCompare == null)
        {
            _errCallback(ErrorInvalidOperator, $"Bad operator for number compare, can't find comparison '{operation}'");
            return (null as Func<IEntity, bool>, "error");
        }

        return (e =>
        {
            var value = e.GetBestValue(fieldName, languages);
            if (value == null) return false;
            try
            {
                var valAsDec = Convert.ToDecimal(value);
                return numberCompare(valAsDec);
            }
            catch
            {
                return false;
            }
        }, "ok");

    });

    private Func<decimal, bool> NumberInnerCompare(string operation, decimal expected, decimal expectedMax, List<decimal> decimals = null)
    {
        switch (operation)
        {
            case OpEquals: return value => value == expected;
            case OpExactly: return value => value == expected;
            case OpNotEquals: return value => value != expected;
            case OpGt: return value => value > expected;
            case OpLt: return value => value < expected;
            case OpGtEquals: return value => value >= expected;
            case OpLtEquals: return value => value <= expected;
            case OpBetween: return value => value >= expected && value <= expectedMax;
            case OpNotBetween: return value => !(value >= expected && value <= expectedMax);
            case OpContains: return value => decimals?.Contains(value) ?? false;
            case OpNotContains: return value => !decimals?.Contains(value) ?? false;
            default: return null;
        }
    }


    /// <summary>
    /// provide all date-time comparison as a prepared function
    /// </summary>
    private Func<IEntity, bool> DateTimeComparison(string fieldName, string operation, string[] languages, string expected) => Log.Func(expected, () =>
    {
        var max = DateTime.MaxValue;
        var expectedDtm = DateTime.MinValue;

        #region handle special case "between" with 2 values
        if (operation == OpBetween || operation == OpNotBetween)
        {
            Log.A("Operator is between or !between");
            var (useBetween, start, end) = BetweenParts(expected);
            if (useBetween)
            {
                DateTime.TryParse(start, out expectedDtm);
                DateTime.TryParse(end, out max);
            }
            else
                operation = OpEquals;
        }
        #endregion

        // get the value (but only if it hasn't been initialized already)
        if (expectedDtm == DateTime.MinValue)
            DateTime.TryParse(expected, out expectedDtm);

        var innerFunc = DateTimeInnerCompare(operation, expectedDtm, max);

        if (innerFunc == null)
        {
            _errCallback(ErrorInvalidOperator, $"Bad operator for datetime compare, can't find comparison '{operation}'");
            return (null as Func<IEntity, bool>, "error");
        }

        return (e =>
        {
            try
            {
                // This will treat null as DateTime.MinValue - because that's also how the null-parameter is parsed when creating the filter
                var value = e.GetBestValue<DateTime>(fieldName, languages);
                return innerFunc(value);
            }
            catch
            {
                return false;
            }
        }, "ok");
    });

    private static Func<DateTime, bool> DateTimeInnerCompare(string operation, DateTime expected, DateTime max)
    {
        switch (operation)
        {
            case OpEquals: return value => value == expected;
            case OpExactly: return value => value == expected;
            case OpNotEquals: return value => value != expected;
            case OpGt: return value => value > expected;
            case OpLt: return value => value < expected;
            case OpGtEquals: return value => value >= expected;
            case OpLtEquals: return value => value <= expected;
            case OpBetween: return value => value >= expected && value <= max;
            case OpNotBetween: return value => !(value >= expected && value <= max);
            default: return null;
        }
    }


    #region "between" helper

    private (bool useBetween, string start, string end) BetweenParts(string expected) => Log.Func(expected, () =>
    {
        expected = expected.ToLowerInvariant();
        var hasAnd = expected.IndexOf(" and ", Ordinal);
        string low = "", high = "";
        if (hasAnd > -1)
        {
            Log.A("has 'and'");
            low = expected.Substring(0, hasAnd).Trim();
            high = expected.Substring(hasAnd + 4).Trim();
            Log.A($"has 'and'. low: {low}, high: {high}");
        }
        else Log.A("No 'and' found, low/high will be empty");

        return (hasAnd > -1, low, high);
    });

    #endregion

}