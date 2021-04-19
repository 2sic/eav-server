using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Return only Entities having a specific value in an Attribute/Property
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(
        NiceName = "Value Filter",
        UiHint = "Keep items which have a property with the expected value",
        Icon = "filter_list",
        Type = DataSourceType.Filter, 
        GlobalName = "ToSic.Eav.DataSources.ValueFilter, ToSic.Eav.DataSources",
        In = new[] { Constants.DefaultStreamNameRequired, Constants.FallbackStreamName },
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.ValueFilter",
        HelpLink = "https://r.2sxc.org/DsValueFilter")]

    public sealed class ValueFilter : DataSourceBase
    {
        private readonly ValueLanguages _valLanguages;

        #region Configuration-properties Attribute, Value, Language, Operator
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.ValueF";

        private const string AttrKey = "Attribute";
        private const string FilterKey = "Value";
        private const string OperatorKey = "Operator";
        private const string TakeKey = "Take";

        /// <summary>
		/// The attribute whose value will be scanned / filtered.
		/// </summary>
		public string Attribute
		{
			get => Configuration[AttrKey];
            set => Configuration[AttrKey] = value;
        }

		/// <summary>
		/// The filter that will be used - for example "Daniel" when looking for an entity w/the value Daniel
		/// </summary>
		public string Value
		{
			get => Configuration[FilterKey];
		    set => Configuration[FilterKey] = value;
		}

		/// <summary>
		/// Language to filter for. At the moment it is not used, or it is trying to find "any"
		/// </summary>
		public string Languages
		{
			get => Configuration[ValueLanguages.LangKey];
		    set => Configuration[ValueLanguages.LangKey] = value;
		}

        /// <summary>
        /// The comparison operator, == by default, many possibilities exist
        /// depending on the original types we're comparing
        /// </summary>
		public string Operator
		{
			get => Configuration[OperatorKey];
            set => Configuration[OperatorKey] = value;
        }

        /// <summary>
        /// Amount of items to take - then stop filtering. For performance optimization.
        /// </summary>
		public string Take
		{
			get => Configuration[TakeKey];
		    set => Configuration[TakeKey] = value;
		}		
        #endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new ValueFilter
		/// </summary>
		[PrivateApi]
		public ValueFilter(ValueLanguages valLanguages)
		{
            Provide(GetValueFilterOrFallback);
		    ConfigMask(AttrKey, "[Settings:Attribute]");
		    ConfigMask(FilterKey, "[Settings:Value]");
		    ConfigMask(OperatorKey, "[Settings:Operator||==]");
		    ConfigMask(TakeKey, "[Settings:Take]");
		    ConfigMask(ValueLanguages.LangKey, ValueLanguages.LanguageSettingsPlaceholder);

            _valLanguages = valLanguages.Init(Log);
        }

        private IImmutableList<IEntity> GetValueFilterOrFallback()
        {
            var callLog = Log.Call<IImmutableList<IEntity>>();

            var res = GetValueFilter();
            if (res.Any()) return callLog("found", res);
            if (In.HasStreamWithItems(Constants.FallbackStreamName))
                return callLog("fallback", In[Constants.FallbackStreamName].List.ToImmutableList());

            return callLog("final", res);
        }


        private IImmutableList<IEntity> GetValueFilter()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            // todo: maybe do something about languages?
            Configuration.Parse();

            Log.Add("applying value filter...");
			_initializedAttrName = Attribute;

            LanguageList = _valLanguages.PrepareLanguageList(Languages, Log);

            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            //var originals = In[Constants.DefaultStreamName].List.ToImmutableList();

            // stop if the list is empty
            if (!originals.Any())
                return wrapLog("empty", originals);


 		    Func<IEntity, bool> compare; // the real comparison method which will be used

            #region if it's a real filter - optimize

		    var op = Operator.ToLowerInvariant();
            if (op == "none" || op == "all")
                compare = e => true; // dummy comparison
            else
            {
                // Find first Entity which has this property being not null
                var firstEntity = Constants.InternalOnlyIsSpecialEntityProperty(_initializedAttrName)
                    ? originals.FirstOrDefault()
                    : originals.FirstOrDefault(x => x.Attributes.ContainsKey(_initializedAttrName) 
                                                    && x.Value(_initializedAttrName) != null);

                // if I can't find any, return empty list
                if (firstEntity == null)
                    return wrapLog("empty", ImmutableArray<IEntity>.Empty);

                // New mechanism because the filter previously ignored internal properties like Modified, EntityId etc.
                // Using .Value should get everything, incl. modified, EntityId, EntityGuid etc.
                var firstAtt = firstEntity.Value(_initializedAttrName);
                
                // 2021-03-29 2dm changed from checking the type-name to actually checking the type
                // this was necessary, because entity-lists were LazyEntities and not "Entity"
                //var netTypeName = firstAtt?.GetType().Name ?? "Null";
                // very special case - since we're using the .net type and not the Attribute.Type,
                // then lazy-entities are marked as LazyEntity or similar, and NOT "Entity"
                //if (netTypeName.Contains(Constants.DataTypeEntity)) netTypeName = Constants.DataTypeEntity;
                
                switch (firstAtt)
                {
                    case bool b:
                        Log.Add("Will apply Boolean comparison");
                        compare = GetBoolComparison(Value);
                        break;
                    case int i:
                    case float f:
                    case decimal d:
                        Log.Add("Will apply Decimal comparison");
                        compare = GetNumberComparison(Value);
                        break;
                    case DateTime d:
                        Log.Add("Will apply DateTime comparison");
                        compare = GetDateTimeComparison(Value);
                        break;
                    case IEnumerable<IEntity> ie:
                    case IEntity e:
                        Log.Add("Would apply entity comparison, but this doesn't work");
                        return wrapLog("error", SetError("Can't apply Value comparison to Relationship",
                            "Can't compare values which contain related entities - use the RelationshipFilter instead."));
                    case string s:
                    case null:  // note: null should never happen, because we only look at entities having non-null in this value
                    default:
                        Log.Add("Will apply String comparison");
                        compare = GetStringComparison(Value);
                        break;
                }
            }
            #endregion

            if (!ErrorStream.IsDefaultOrEmpty)
                return wrapLog("error", ErrorStream);

            return wrapLog("ok", GetFilteredWithLinq(originals, compare));
            // The following version has more logging, activate in serious cases
            // Note that the code might not be 100% identical, but it should help find issues
		}



        /// <summary>
        /// Provide all the string comparison functionality as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Func<IEntity, bool> GetStringComparison(string original)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>(original);
            var operation = Operator.ToLowerInvariant();

            var stringComparison = new Dictionary<string, Func<object, bool>>
            {
                {"==", value => value != null && string.Equals(value.ToString(), original, StringComparison.InvariantCultureIgnoreCase)        },
                { "===", value => value != null && value.ToString() == original}, // case sensitive, full equal
                {"!=", value => value != null && !string.Equals(value.ToString(), original, StringComparison.InvariantCultureIgnoreCase)},
                {"contains", value => value?.ToString().IndexOf(original, StringComparison.OrdinalIgnoreCase) > -1 },
                {"!contains", value => value?.ToString().IndexOf(original, StringComparison.OrdinalIgnoreCase) == -1},
                {"begins", value => value?.ToString().IndexOf(original, StringComparison.OrdinalIgnoreCase) == 0 },
                {"all", value => true }
            };

            if (!stringComparison.ContainsKey(operation))
            {
                SetError("Invalid Operator", $"Bad operator for string compare, can't find comparison '{operation}'");
                return wrapLog("error", null);
            }

            var stringCompare = stringComparison[operation];
            return wrapLog("ok", e => stringCompare(e.GetBestValue(_initializedAttrName, LanguageList)));
        }

        /// <summary>
        /// Provide all bool-compare functionality as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Func<IEntity, bool> GetBoolComparison(string original)
        {
            var wrapLog = Log.Call(original);

            var boolFilter = bool.Parse(original);

            var operation = Operator.ToLowerInvariant();
            switch (operation)
            {
                case "==":
                case "===":
                    wrapLog("ok");
                    return e =>
                    {
                        var value = e.GetBestValue(_initializedAttrName, LanguageList);
                        return value as bool? == boolFilter;
                    };
                case "!=":
                    wrapLog("ok");
                    return e =>
                    {
                        var value = e.GetBestValue(_initializedAttrName, LanguageList);
                        return value as bool? != boolFilter;
                    };
            }

            SetError("Invalid Operator",message: $"Bad operator for boolean compare, can't find comparison '{operation}'");
            wrapLog("error");
            return null;
        }



        #region "between" helper
        private Tuple<bool, string, string> BetweenParts(string original)
        {
            var wrapLog = Log.Call(original);
            original = original.ToLowerInvariant();
            var hasAnd = original.IndexOf(" and ", StringComparison.Ordinal);
            string low = "", high = "";
            if (hasAnd > -1)
            {
                Log.Add("has 'and'");
                low = original.Substring(0, hasAnd).Trim();
                high = original.Substring(hasAnd + 4).Trim();
                Log.Add($"has 'and'. low: {low}, high: {high}");
            }
            else Log.Add("No 'and' found, low/high will be empty");

            wrapLog("ok");
            return new Tuple<bool, string, string>(hasAnd > -1, low, high);
        }
        #endregion 


        /// <summary>
        /// provide all date-time comparison as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Func<IEntity, bool> GetDateTimeComparison(string original)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>(original);

            var operation = Operator.ToLowerInvariant();
            DateTime max = DateTime.MaxValue,
                referenceDateTime = DateTime.MinValue;
            
            #region handle special case "between" with 2 values
            if (operation == "between" || operation == "!between")
            {
                Log.Add("Operator is between or !between");
                var parts = BetweenParts(original);
                if (parts.Item1)
                {
                    DateTime.TryParse(parts.Item2, out referenceDateTime);
                    DateTime.TryParse(parts.Item3, out max);
                }
                else
                    operation = "==";   
            }
            #endregion

            // get the value (but only if it hasn't been initialized already)
            if (referenceDateTime == DateTime.MinValue)
                DateTime.TryParse(original, out referenceDateTime);

            var dateComparisons = new Dictionary<string, Func<DateTime, bool>>
            {
                {"==", value => value == referenceDateTime},
                {"===", value => value == referenceDateTime},
                {"!=", value => value != referenceDateTime},
                {">", value => value > referenceDateTime},
                {"<", value => value < referenceDateTime},
                {">=", value => value >= referenceDateTime},
                {"<=", value => value <= referenceDateTime},
                {"between", value => value >= referenceDateTime && value <= max },
                {"!between", value => !(value >= referenceDateTime && value <= max) },
            };

            if (!dateComparisons.ContainsKey(operation))
            {
                SetError("Invalid Operator", $"Bad operator for datetime compare, can't find comparison '{operation}'");
                return wrapLog("error", null);
            }

            var dateTimeCompare = dateComparisons[operation];

            wrapLog("ok", null);
            return e => {
                var value = e.GetBestValue(_initializedAttrName, LanguageList);
                try
                {
                    // treat null as DateTime.MinValue - because that's also how the null-parameter is parsed when creating the filter
                    var valAsDec = value == null ? DateTime.MinValue : Convert.ToDateTime(value);
                    return dateTimeCompare(valAsDec);
                }
                catch
                {
                    return false;
                }
            };
        }



        /// <summary>
        /// provide all number-compare functionality as prepared/precompiled functions
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Func<IEntity, bool> GetNumberComparison(string original)
        {
            var wrapLog = Log.Call<Func<IEntity, bool>>(original);

            var operation = Operator.ToLowerInvariant();
        
            var max = decimal.MaxValue;
            var numberFilter = decimal.MinValue;

            #region check for special case "between" with two values to compare
            if (operation == "between" || operation == "!between")
            {
                Log.Add("Operator is between or !between");
                var parts = BetweenParts(original);
                if (parts.Item1)
                {
                    decimal.TryParse(parts.Item2, out numberFilter);
                    decimal.TryParse(parts.Item3, out max);
                }
                else
                    operation = "==";
            }
            #endregion

            // get the value (but only if it hasn't been initialized already)
            if (numberFilter == decimal.MinValue)
                decimal.TryParse(original, out numberFilter);

            var numComparisons = new Dictionary<string, Func<decimal, bool>>
            {
                {"==", value => value == numberFilter},
                {"===", value => value == numberFilter},
                {"!=", value => value != numberFilter},
                {">", value => value > numberFilter},
                {"<", value => value < numberFilter},
                {">=", value => value >= numberFilter},
                {"<=", value => value <= numberFilter},
                {"between", value => value >= numberFilter && value <= max },
                {"!between", value => !(value >= numberFilter && value <= max) },
            };

            if (!numComparisons.ContainsKey(operation))
            {
                SetError("Invalid Operator", $"Bad operator for number compare, can't find comparison '{operation}'");
                return wrapLog("error", null);
            }

            var numberCompare = numComparisons[operation];
            wrapLog("ok", null);
            return e =>
            {
                var value = e.GetBestValue(_initializedAttrName, LanguageList);
                if (value == null)
                    return false;
                try
                {
                    var valAsDec = Convert.ToDecimal(value);
                    return numberCompare(valAsDec);
                }
                catch
                {
                    return false;
                }
            };

        }

        /// <summary>
        /// The internal language list used to lookup values.
        /// It's internal, to allow testing/debugging from the outside
        /// </summary>
        [PrivateApi] internal string[] LanguageList { get; private set; }
	    [PrivateApi] private string _initializedAttrName;

	    private ImmutableArray<IEntity> GetFilteredWithLinq(IEnumerable<IEntity> originals, Func<IEntity, bool> compare)
        {
            var wrapLog = Log.Call<ImmutableArray<IEntity>>();
            try
            {
                var op = Operator.ToLowerInvariant();
                IEnumerable<IEntity> results;
                switch (op)
                {
                    case "all":
                        results = from e in originals select e;
                        break;
                    case "none":
                        results = new List<IEntity>();
                        break;
                    default:
                        results = originals.Where(compare);
                        break;
                }
                if (int.TryParse(Take, out var tk))
	                results = results.Take(tk);
	            return wrapLog("ok", results.ToImmutableArray());
	        }
	        catch (Exception ex)
            {
                return wrapLog("error", SetError("Unexpected Error",
                    "Experienced error while executing the filter LINQ. " +
                    "Probably something with type-mismatch or the same field using different types or null. " +
                    "The exception was logged to Insights.",
                    ex));
            }
	    }


	    /// <summary>
	    /// A helper function to apply the filter without LINQ - ideal when trying to debug exactly what value crashed
	    /// </summary>
	    /// <param name="inList"></param>
	    /// <param name="attr"></param>
	    /// <param name="lang"></param>
	    /// <param name="filter"></param>
	    /// <returns></returns>
	    // ReSharper disable once UnusedMember.Local
	    private IEnumerable<IEntity> GetFilteredWithLoop(IEnumerable<IEntity> inList, string attr, string lang, string filter)
	    {
            var result = new List<IEntity>();
            var langArr = new[] { lang };
            foreach (var res in inList)
            //try
            //{
                //if (res.Value[attr][lang].ToString() == filter)
                if ((res.GetBestValue(attr, langArr) ?? "").ToString() == filter)
                    result.Add(res);
            //}
            //catch { }
            return result;
	    }
		
	}
}