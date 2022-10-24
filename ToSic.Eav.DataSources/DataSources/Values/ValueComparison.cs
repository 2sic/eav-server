using System;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Lib.Documentation;
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

        public Func<IEntity, bool> GetComparison(ValueTypes type, /*object firstValue,*/ string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Fn<Func<IEntity, bool>>();
            operation = operation.ToLowerInvariant();

            // First try to figure out based on known type
            switch (type)
            {
                case ValueTypes.Boolean:
                    return wrapLog.Return(BoolComparison(fieldName, operation, languages, expected), "bool comparison");
                case ValueTypes.DateTime:
                    return wrapLog.Return(DateTimeComparison(fieldName, operation, languages, expected), "datetime comparison");
                case ValueTypes.Number:
                    return wrapLog.Return(NumberComparison(fieldName, operation, languages, expected), "decimal comparison");
                case ValueTypes.Entity:
                    Log.A("Would apply entity comparison, but this doesn't work");
                    _errCallback("Can't apply Value comparison to Relationship",
                        "Can't compare values which contain related entities - use the RelationshipFilter instead.");
                    return wrapLog.ReturnNull("error");
                case ValueTypes.Undefined:
                case ValueTypes.Hyperlink:
                case ValueTypes.String:
                case ValueTypes.Empty:
                case ValueTypes.Custom:
                case ValueTypes.Json:
                default:
                    return wrapLog.Return(StringComparison(fieldName, operation, languages, expected), "string comparison");
            }

            // Fallback - if known type didn't work, try based on the object data
            // 2022-03-09 2dm disabled this, I believe this will never happen based on the changes above
            // If all works well, remove mid of 2022
            //switch (firstValue)
            //{
            //    case bool _:
            //        return wrapLog("bool comparison", BoolComparison(fieldName, operation, languages, expected));
            //    case int _:
            //    case float _:
            //    case decimal _:
            //        return wrapLog("decimal comparison", NumberComparison(fieldName, operation, languages, expected));
            //    case DateTime _:
            //        return wrapLog("datetime comparison", DateTimeComparison(fieldName, operation, languages, expected));
            //    case IEnumerable<IEntity> _:
            //    case IEntity _:
            //        Log.A("Would apply entity comparison, but this doesn't work");
            //        _errCallback("Can't apply Value comparison to Relationship",
            //            "Can't compare values which contain related entities - use the RelationshipFilter instead.");
            //        return wrapLog("error", null);
            //    case string _:
            //    case null:  // note: null should never happen, because we only look at entities having non-null in this value
            //    default:
            //        return wrapLog("string comparison", StringComparison(fieldName, operation, languages, expected));
            //}
        }


        /// <summary>
        /// Provide all the string comparison functionality as a prepared function
        /// </summary>
        private Func<IEntity, bool> StringComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Fn<Func<IEntity, bool>>(expected);

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
                return wrapLog.ReturnNull("error");
            }

            return wrapLog.ReturnAsOk(e => stringCompare(e.GetBestValue(fieldName, languages)));
        }


        /// <summary>
        /// Provide all bool-compare functionality as a prepared function
        /// </summary>
        private Func<IEntity, bool> BoolComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Fn<Func<IEntity, bool>>(expected);

            var boolFilter = bool.Parse(expected);

            switch (operation)
            {
                case OpEquals:
                case OpExactly:
                    return wrapLog.ReturnAsOk(e => e.GetBestValue(fieldName, languages) as bool? == boolFilter);
                case OpNotEquals:
                    return wrapLog.ReturnAsOk(e => e.GetBestValue(fieldName, languages) as bool? != boolFilter);
            }

            _errCallback(ErrorInvalidOperator, $"Bad operator for boolean compare, can't find comparison '{operation}'");
            return wrapLog.ReturnNull("error");
        }


        /// <summary>
        /// provide all number-compare functionality as prepared/precompiled functions
        /// </summary>
        private Func<IEntity, bool> NumberComparison(string fieldName, string operation, string[] languages, string expected)
        {
            var wrapLog = Log.Fn<Func<IEntity, bool>>(expected);

            var minOrExpected = decimal.MinValue;
            var max = decimal.MaxValue;

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

            // get the value (but only if it hasn't been initialized already)
            if (minOrExpected == decimal.MinValue)
                decimal.TryParse(expected, out minOrExpected);

            var numberCompare = NumberInnerCompare(operation, minOrExpected, max) ;
            if (numberCompare == null)
            {
                _errCallback(ErrorInvalidOperator, $"Bad operator for number compare, can't find comparison '{operation}'");
                return wrapLog.ReturnNull("error");
            }

            return wrapLog.ReturnAsOk(e =>
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
            var wrapLog = Log.Fn<Func<IEntity, bool>>(expected);

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
                return wrapLog.ReturnNull("error");
            }

            return wrapLog.ReturnAsOk(e =>
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
            var wrapLog = Log.Fn<(bool, string, string)>(expected);
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

            return wrapLog.ReturnAsOk((hasAnd > -1, low, high));
        }
        #endregion 

    }
}
