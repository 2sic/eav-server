using static System.StringComparison;
using static ToSic.Eav.DataSources.CompareOperators;

namespace ToSic.Eav.DataSources;

[PrivateApi]
internal class ValueComparison: HelperBase
{
    internal ValueComparison(Action<string, string> errCallback, ILog parentLog): base(parentLog, "Eav.DsCmMk")
    {
        _errCallback = errCallback ?? throw new ArgumentNullException(nameof(errCallback));
    }

    private readonly Action<string, string> _errCallback;

    public Func<IEntity, bool> GetComparison(ValueTypes type, string fieldName, string operation, string[] languages, string expected)
    {
        var l = Log.Fn<Func<IEntity, bool>>();
        operation = operation.ToLowerInvariant();

        // First try to figure out based on known type
        switch (type)
        {
            case ValueTypes.Boolean:
                return l.Return(BoolComparison(fieldName, operation, languages, expected), "bool comparison");
            case ValueTypes.DateTime:
                return l.Return(DateTimeComparison(fieldName, operation, languages, expected), "datetime comparison");
            case ValueTypes.Number:
                return l.Return(NumberComparison(fieldName, operation, languages, expected), "decimal comparison");
            case ValueTypes.Entity:
                l.A("Would apply entity comparison, but this doesn't work");
                _errCallback("Can't apply Value comparison to Relationship",
                    "Can't compare values which contain related entities - use the RelationshipFilter instead.");
                return l.ReturnNull("error");
            case ValueTypes.Undefined:
            case ValueTypes.Hyperlink:
            case ValueTypes.String:
            case ValueTypes.Empty:
            case ValueTypes.Custom:
            case ValueTypes.Json:
            default:
                return l.Return(StringComparison(fieldName, operation, languages, expected), "string comparison");
        }
    }


    /// <summary>
    /// Provide all the string comparison functionality as a prepared function
    /// </summary>
    private Func<IEntity, bool> StringComparison(string fieldName, string operation, string[] languages, string expected)
    {
        var l = Log.Fn<Func<IEntity, bool>>(expected);

        var stringCompare = PickStringCompare(operation);

        if (stringCompare == null)
        {
            _errCallback(ErrorInvalidOperator, $"Bad operator for string compare, can't find comparison '{operation}'");
            return l.ReturnNull("error");
        }

        return l.Return(e => stringCompare(e.Get(fieldName, languages: languages)), "ok");

        Func<object, bool> PickStringCompare(string op) => op switch
        {
            OpEquals => value => value != null && string.Equals(value.ToString(), expected, InvariantCultureIgnoreCase),
            OpExactly => value => value != null && value.ToString() == expected, // case sensitive: return full equal
            OpNotEquals => value => value != null && !string.Equals(value.ToString(), expected, InvariantCultureIgnoreCase),
            OpContains => value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) > -1,
            OpNotContains => value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) == -1,
            OpBegins => value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) == 0,
            OpAll => value => true,
            _ => null
        };
    }


    /// <summary>
    /// Provide all bool-compare functionality as a prepared function
    /// </summary>
    private Func<IEntity, bool> BoolComparison(string fieldName, string operation, string[] languages, string expected)
    {
        var l = Log.Fn<Func<IEntity, bool>>(expected);
        var boolFilter = bool.Parse(expected);

        switch (operation)
        {
            case OpEquals:
            case OpExactly: return l.Return(e => e.Get(fieldName, languages: languages) as bool? == boolFilter, "ok");
            case OpNotEquals: return l.Return(e => e.Get(fieldName, languages: languages) as bool? != boolFilter, "ok");
        }

        _errCallback(ErrorInvalidOperator, $"Bad operator for boolean compare, can't find comparison '{operation}'");
        return l.ReturnNull("error");
    }


    /// <summary>
    /// provide all number-compare functionality as prepared/precompiled functions
    /// </summary>
    private Func<IEntity, bool> NumberComparison(string fieldName, string operation, string[] languages, string expected)
    {
        var l = Log.Fn<Func<IEntity, bool>>(expected);

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
            decimals = [];
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
            return l.ReturnNull("error");
        }

        return l.Return(e =>
        {
            var value = e.Get(fieldName, languages: languages);
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

    }

    private Func<decimal, bool> NumberInnerCompare(string operation, decimal expected, decimal expectedMax, List<decimal> decimals = null)
    {
        var l = Log.Fn<Func<decimal, bool>>($"{expected}");

        Func<decimal, bool> finalFn = operation switch
        {
            OpEquals => value => value == expected,
            OpExactly => value => value == expected,
            OpNotEquals => value => value != expected,
            OpGt => value => value > expected,
            OpLt => value => value < expected,
            OpGtEquals => value => value >= expected,
            OpLtEquals => value => value <= expected,
            OpBetween => value => value >= expected && value <= expectedMax,
            OpNotBetween => value => !(value >= expected && value <= expectedMax),
            OpContains => value => decimals?.Contains(value) ?? false,
            OpNotContains => value => !decimals?.Contains(value) ?? false,
            _ => null
        };

        return l.Return(finalFn);
    }


    /// <summary>
    /// provide all date-time comparison as a prepared function
    /// </summary>
    private Func<IEntity, bool> DateTimeComparison(string fieldName, string operation, string[] languages, string expected)
    {
        var l = Log.Fn<Func<IEntity, bool>>(expected);

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
            return l.ReturnNull("error");
        }

        return l.Return(e =>
        {
            try
            {
                // This will treat null as DateTime.MinValue - because that's also how the null-parameter is parsed when creating the filter
                var value = e.Get<DateTime>(fieldName, languages: languages);
                return innerFunc(value);
            }
            catch
            {
                return false;
            }
        }, "ok");
    }

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

    private (bool useBetween, string start, string end) BetweenParts(string expected)
    {
        var l = Log.Fn<(bool useBetween, string start, string end)>(expected);
        expected = expected.ToLowerInvariant();
        var hasAnd = expected.IndexOf(" and ", Ordinal);
        string low = "", high = "";
        if (hasAnd > -1)
        {
            l.A("has 'and'");
            low = expected.Substring(0, hasAnd).Trim();
            high = expected.Substring(hasAnd + 4).Trim();
            l.A($"has 'and'. low: {low}, high: {high}");
        }
        else l.A("No 'and' found, low/high will be empty");

        return l.Return((hasAnd > -1, low, high));
    }

    #endregion

}