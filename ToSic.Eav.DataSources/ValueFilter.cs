using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.DataSources.Exceptions;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Return only Entities having a specific value in an Attribute
    /// </summary>
    [PipelineDesigner]
    [DataSourceProperties(Type = DataSourceType.Filter)]

    public sealed class ValueFilter : BaseDataSource
    {
        #region Configuration-properties Attribute, Value, Language, Operator
        public override string LogId => "DS.ValueF";

        private const string AttrKey = "Attribute";
        private const string FilterKey = "Value";
        private const string LangKey = "Language";
        private const string OperatorKey = "Operator";
        private const string TakeKey = "Take";

        /// <summary>
		/// The attribute whoose value will be filtered
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
		public ValueFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetEntitiesOrFallback));
			Configuration.Add(AttrKey, "[Settings:Attribute]");
			Configuration.Add(FilterKey, "[Settings:Value]");
            Configuration.Add(OperatorKey, "[Settings:Operator||==]");
            Configuration.Add(TakeKey, "[Settings:Take]");
			Configuration.Add(LangKey, "Default"); // "[Settings:Language|Any]"); // use setting, but by default, expect "any"

            CacheRelevantConfigurations = new[] { AttrKey, FilterKey, LangKey };
        }

        private IEnumerable<IEntity> GetEntitiesOrFallback()
        {
            var res = GetEntities();
            // ReSharper disable PossibleMultipleEnumeration
            if (!res.Any())
                if (In.ContainsKey(Constants.FallbackStreamName) && In[Constants.FallbackStreamName] != null &&
                    In[Constants.FallbackStreamName].LightList.Any())
                {
                    Log.Add("will return fallback stream");
                    res = In[Constants.FallbackStreamName].LightList;
                }
            
            return res;
            // ReSharper restore PossibleMultipleEnumeration
        }


        private IEnumerable<IEntity> GetEntities()
		{
			// todo: maybe do something about languages?
			EnsureConfigurationIsLoaded();

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

            var originals = In[Constants.DefaultStreamName].LightList.ToList();

            #region stop if the list is empty
            if (!originals.Any()) 
		        return originals;
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
                    : originals.FirstOrDefault(x => x.Attributes.ContainsKey(_initializedAttrName));

                // if I can't find any, return empty list
                if (firstEntity == null)
                    return new List<IEntity>();

                // New mechanism because the filter ignores internal properties like Modified, EntityId etc.
                var firstAtt = firstEntity.GetBestValue(_initializedAttrName);  // this should get everything, incl. modified, EntityId, EntityGuid etc.
                var netTypeName = firstAtt.GetType().Name;
                switch (netTypeName)
                {
                    case "Boolean": // todo: find some constant for this
                        compare = GetBoolComparison(Value);
                        break;
                    case "Decimal":
                        compare = GetNumberComparison(Value);
                        break;
                    case "DateTime":
                        compare = GetDateTimeComparison(Value);
                        break;
                    case "Entity":
                        throw new Exception("can't compare values which are related entities - use the RelationshipFilter instead");
                    // ReSharper disable once RedundantCaseLabel
                    case "String":
                    default:
                        compare = GetStringComparison(Value);
                        break;
                }


            }
            #endregion

            return GetFilteredWithLinq(originals, compare);
		    //_results = GetFilteredWithLoop(originals, compare);
		}

        /// <summary>
        /// Provide all entity-compare functionality as a prepared function
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
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

            var dateComparisons = new Dictionary<string, Func<DateTime, bool>>()
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
                if (value == null)
                    return false;
                try
                {
                    var valAsDec = Convert.ToDateTime(value);
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

            var numComparisons = new Dictionary<string, Func<decimal, bool>>()
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

	    private IEnumerable<IEntity> GetFilteredWithLinq(IEnumerable<IEntity> originals, Func<IEntity, bool> compare)//, string attr, string lang)//, string filter)
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
                        results = (from e in originals
                            where compare(e)
                            select e);
                        break;
                }
                if (int.TryParse(Take, out var tk))
	                results = results.Take(tk);
	            return results;
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
	    private IDictionary<int, IEntity> GetFilteredWithLoop(IDictionary<int, IEntity> inList, string attr, string lang, string filter)
	    {
            var result = new Dictionary<int, IEntity>();
            var langArr = new[] { lang };
            foreach (var res in inList)
                //try
                //{
                    //if (res.Value[attr][lang].ToString() == filter)
                    if ((res.Value.GetBestValue(attr, langArr) ?? "").ToString() == filter)
                        result.Add(res.Key, res.Value);
                //}
                //catch { }
	        return result;
	    }
		
	}
}