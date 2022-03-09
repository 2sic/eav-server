using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using static System.StringComparison;
using static ToSic.Eav.DataSources.CompareOperators;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public class ValueComparison: HasLog
    {
        internal ValueComparison(Action<string, string> errCallback, ILog parentLog): base("Eav.DsCmMk", parentLog)
        {
            _errCallback = errCallback ?? throw new ArgumentNullException(nameof(errCallback));
        }

        private readonly Action<string, string> _errCallback; // (string title, string message);

        public Func<IEntity, bool> GetComparison(object firstValue, string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>();
            operation = operation.ToLowerInvariant();

            switch (firstValue)
            {
                case bool _:
                    return wrapLog("bool comparison", BoolComparison(fieldName, operation, languages, expected));
                case int _:
                case float _:
                case decimal _:
                    return wrapLog("decimal comparison", NumberComparison(fieldName, operation, languages, expected));
                case DateTime _:
                    return wrapLog("datetime comparison", DateTimeComparison(fieldName, operation, languages, expected));
                case IEnumerable<IEntity> _:
                case IEntity _:
                    Log.Add("Would apply entity comparison, but this doesn't work");
                    _errCallback("Can't apply Value comparison to Relationship",
                        "Can't compare values which contain related entities - use the RelationshipFilter instead.");
                    return wrapLog("error", null);
                case string _:
                case null:  // note: null should never happen, because we only look at entities having non-null in this value
                default:
                    return wrapLog("string comparison", StringComparison(fieldName, operation, languages, expected));
            }
        }


        /// <summary>
        /// Provide all the string comparison functionality as a prepared function
        /// </summary>
        private Func<IEntity, bool> StringComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>(expected);

            var stringComparison = new Dictionary<string, Func<object, bool>>
            {
                { OpEquals, value => value != null && string.Equals(value.ToString(), expected, InvariantCultureIgnoreCase)        },
                { OpExactly, value => value != null && value.ToString() == expected}, // case sensitive, full equal
                { OpNotEquals, value => value != null && !string.Equals(value.ToString(), expected, InvariantCultureIgnoreCase)},
                { OpContains, value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) > -1 },
                { OpNotContains, value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) == -1},
                { OpBegins, value => value?.ToString().IndexOf(expected, InvariantCultureIgnoreCase) == 0 },
                { OpAll, value => true }
            };

            if (!stringComparison.ContainsKey(operation))
            {
                _errCallback(ErrorInvalidOperator, $"Bad operator for string compare, can't find comparison '{operation}'");
                return wrapLog("error", null);
            }

            var stringCompare = stringComparison[operation];
            return wrapLog("ok", e => stringCompare(e.GetBestValue(fieldName, languages)));
        }


        /// <summary>
        /// Provide all bool-compare functionality as a prepared function
        /// </summary>
        private Func<IEntity, bool> BoolComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Call(expected);

            var boolFilter = bool.Parse(expected);

            switch (operation)
            {
                case OpEquals:
                case OpExactly:
                    wrapLog("ok");
                    return e => e.GetBestValue(fieldName, languages) as bool? == boolFilter;
                case OpNotEquals:
                    wrapLog("ok");
                    return e => e.GetBestValue(fieldName, languages) as bool? != boolFilter;
            }

            _errCallback(ErrorInvalidOperator, $"Bad operator for boolean compare, can't find comparison '{operation}'");
            wrapLog("error");
            return null;
        }


        /// <summary>
        /// provide all number-compare functionality as prepared/precompiled functions
        /// </summary>
        private Func<IEntity, bool> NumberComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>(expected);

            var minOrExpected = decimal.MinValue;
            var max = decimal.MaxValue;

            #region check for special case "between" with two values to compare
            if (operation == OpBetween || operation == OpNotBetween)
            {
                Log.Add("Operator is between or !between");
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

            // get the value (but only if it hasn't been initialized already)
            if (minOrExpected == decimal.MinValue)
                decimal.TryParse(expected, out minOrExpected);

            var numberCompare = NumberInnerCompare(operation, minOrExpected, max) ;
            if (numberCompare == null)
            {
                _errCallback(ErrorInvalidOperator, $"Bad operator for number compare, can't find comparison '{operation}'");
                return wrapLog("error", null);
            }

            return wrapLog("ok", e =>
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
            });

        }

        private Func<decimal, bool> NumberInnerCompare(string operation, decimal expected, decimal expectedMax)
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
                default: return null;
            }
        }


        /// <summary>
        /// provide all date-time comparison as a prepared function
        /// </summary>
        private Func<IEntity, bool> DateTimeComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>(expected);

            var max = DateTime.MaxValue;
            var expectedDtm = DateTime.MinValue;

            #region handle special case "between" with 2 values
            if (operation == OpBetween || operation == OpNotBetween)
            {
                Log.Add("Operator is between or !between");
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
                return wrapLog("error", null);
            }

            return wrapLog("ok", e =>
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
            });
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
            var wrapLog = Log.Call(expected);
            expected = expected.ToLowerInvariant();
            var hasAnd = expected.IndexOf(" and ", Ordinal);
            string low = "", high = "";
            if (hasAnd > -1)
            {
                Log.Add("has 'and'");
                low = expected.Substring(0, hasAnd).Trim();
                high = expected.Substring(hasAnd + 4).Trim();
                Log.Add($"has 'and'. low: {low}, high: {high}");
            }
            else Log.Add("No 'and' found, low/high will be empty");

            wrapLog("ok");
            return (hasAnd > -1, low, high);
        }
        #endregion 

    }
}
