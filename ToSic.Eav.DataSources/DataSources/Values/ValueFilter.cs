using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Exceptions;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Return only Entities having a specific value in an Attribute
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.ValueFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Filter, 
        In = new[] { Constants.DefaultStreamName, Constants.FallbackStreamName },
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.ValueFilter",
        HelpLink = "https://r.2sxc.org/DsValueFilter")]

    public sealed class ValueFilter : DataSourceBase
    {
        #region Configuration-properties Attribute, Value, Language, Operator
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.ValueF";

        private const string AttrKey = "Attribute";
        private const string FilterKey = "Value";
        private const string LangKey = "Language";
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
			get => Configuration[LangKey];
		    set => Configuration[LangKey] = value;
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
		public ValueFilter()
		{
			Provide(GetValueFilterOrFallback);
		    ConfigMask(AttrKey, "[Settings:Attribute]");
		    ConfigMask(FilterKey, "[Settings:Value]");
		    ConfigMask(OperatorKey, "[Settings:Operator||==]");
		    ConfigMask(TakeKey, "[Settings:Take]");
		    ConfigMask(LangKey, "Default");
        }

        private IImmutableList<IEntity> GetValueFilterOrFallback()
        {
            var res = GetValueFilter();
            // ReSharper disable PossibleMultipleEnumeration
            if (res.Any()) return res;
            if (In.HasStreamWithItems(Constants.FallbackStreamName))
            {
                Log.Add("will return fallback stream");
                res = In[Constants.FallbackStreamName].Immutable;
            }

            return res;
            // ReSharper restore PossibleMultipleEnumeration
        }


        private IImmutableList<IEntity> GetValueFilter()
        {
            var wrapLog = Log.Call();
            // todo: maybe do something about languages?
            Configuration.Parse();

            Log.Add("applying value filter...");
			_initializedAttrName = Attribute;

            #region do language checks and finish initialization
			var lang = Languages.ToLower();
            if(lang != "default")
				throw  new Exception("Can't filter for languages other than 'default'");
            if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");
		    _initializedLangs = new[] { lang };
            #endregion

            var originals = In[Constants.DefaultStreamName].Immutable;

            #region stop if the list is empty

            if (!originals.Any())
            {
                wrapLog("empty");
                return originals;
            }
            #endregion; 

 		    Func<IEntity, bool> compare; // the real comparison method which will be used

            #region if it's a real filter - optimize

		    var op = Operator.ToLower();
            if (op == "none" || op == "all")
                compare = e => true; // dummy comparison
            else
            {

                var firstEntity = Constants.InternalOnlyIsSpecialEntityProperty(_initializedAttrName)
                    ? originals.FirstOrDefault()
                    : originals.FirstOrDefault(x => x.Attributes.ContainsKey(_initializedAttrName) && x.GetBestValue(_initializedAttrName) != null);

                // if I can't find any, return empty list
                if (firstEntity == null)
                    return ImmutableArray<IEntity>.Empty; //new List<IEntity>();

                // New mechanism because the filter ignores internal properties like Modified, EntityId etc.
                var firstAtt = firstEntity.GetBestValue(_initializedAttrName);  // this should get everything, incl. modified, EntityId, EntityGuid etc.
                var netTypeName = firstAtt?.GetType().Name ?? "Null";
                switch (netTypeName)
                {
                    case Constants.DataTypeBoolean:
                        Log.Add("Will apply Boolean comparison");
                        compare = GetBoolComparison(Value);
                        break;
                    case "Decimal":
                        Log.Add("Will apply Decimal comparison");
                        compare = GetNumberComparison(Value);
                        break;
                    case Constants.DataTypeDateTime:
                        Log.Add("Will apply DateTime comparison");
                        compare = GetDateTimeComparison(Value);
                        break;
                    case Constants.DataTypeEntity:
                        Log.Add("Would apply entity comparison, but this doesn't work");
                        throw new Exception("can't compare values which are related entities - use the RelationshipFilter instead");
                    // ReSharper disable once RedundantCaseLabel
                    case Constants.DataTypeString:
                    // ReSharper disable once RedundantCaseLabel
                    case "Null":
                    default:
                        Log.Add("Will apply String comparison");
                        compare = GetStringComparison(Value);
                        break;
                }


            }
            #endregion

            wrapLog("ok");
            return GetFilteredWithLinq(originals, compare);
            // The following version has more logging, activate in serious cases
            // Note that the code might not be 100% identical, but it should help find issues
		    //_results = GetFilteredWithLoop(originals, compare);
		}

        /// <summary>
        /// Provide all entity-compare functionality as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        private Func<IEntity, bool> GetEntityComparison(string original)
        {
            var boolFilter = bool.Parse(original);

            var operation = Operator.ToLower();
            switch (operation)
            {
                case "==":
                case "===":
                    return e =>
                    {
                        var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
                        return value as bool? == boolFilter;
                    };
                case "!=":
                    return e =>
                    {
                        var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
                        return value as bool? != boolFilter;
                    };
            }

            throw new Exception("Wrong operator for boolean compare, can't find comparison for '" + operation + "'");
        }



        /// <summary>
        /// Provide all the string comparison functionality as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Func<IEntity, bool> GetStringComparison(string original)
        {
            var operation = Operator.ToLower();

            var stringComparison = new Dictionary<string, Func<object, bool>>()
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
                throw new Exception("Wrong operator for string compare, can't find comparison for '" + operation + "'");

            var stringCompare = stringComparison[operation];
            return e 
                => stringCompare(e.GetBestValue(_initializedAttrName, _initializedLangs)); 

        }

        /// <summary>
        /// Provide all bool-compare functionality as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Func<IEntity, bool> GetBoolComparison(string original)
        {
            var boolFilter = bool.Parse(original);

            var operation = Operator.ToLower();
            switch (operation)
            {
                case "==":
                case "===":
                    return e =>
                    {
                        var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
                        return value as bool? == boolFilter;
                    };
                case "!=":
                    return e =>
                    {
                        var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
                        return value as bool? != boolFilter;
                    };
            }

            throw new Exception("Wrong operator for boolean compare, can't find comparison for '" + operation + "'");
        }



        #region "between" helper
        private Tuple<bool, string, string> BetweenParts(string original)
        {
            original = original.ToLower();
            var hasAnd = original.IndexOf(" and ", StringComparison.Ordinal);
            string low = "", high = "";
            if (hasAnd > -1)
            {
                low = original.Substring(0, hasAnd).Trim();
                high = original.Substring(hasAnd + 4).Trim();
            }
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
            var operation = Operator.ToLower();
            DateTime max = DateTime.MaxValue,
                referenceDateTime = DateTime.MinValue;
            
            #region handle special case "between" with 2 values
            if (operation == "between" || operation == "!between")
            {
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

            if(!dateComparisons.ContainsKey(operation))
                throw new Exception("Wrong operator for datetime compare, can't find comparison for '" + operation + "'");

            var dateTimeCompare = dateComparisons[operation];

            return e => {
                var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
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
            var operation = Operator.ToLower();
        
            var max = decimal.MaxValue;
            var numberFilter = decimal.MinValue;

            #region check for special case "between" with two values to compare
            if (operation == "between" || operation == "!between")
            {
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

            if(!numComparisons.ContainsKey(operation))
                throw new Exception("Wrong operator for number compare, can't find comparison for '" + operation + "'");

            var numberCompare = numComparisons[operation];
            return e =>
            {
                var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
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


	    private string[] _initializedLangs;
	    private string _initializedAttrName;

	    private ImmutableArray<IEntity> GetFilteredWithLinq(IEnumerable<IEntity> originals, Func<IEntity, bool> compare)
        {
            try
            {
                var op = Operator.ToLower();
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
	            return results.ToImmutableArray();//.ToList();
	        }
	        catch (Exception ex)
	        {
	            throw new DataSourceException(
	                "Experienced error in ValueFilter while executing the filter LINQ. Probably something with type-missmatch or the same field using different types or null.",
	                ex);
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