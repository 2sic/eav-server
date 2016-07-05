using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using ToSic.Eav.DataSources.Exceptions;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Return only Entities having a specific value in an Attribute
    /// </summary>
    [PipelineDesigner]
    public sealed class ValueFilter : BaseDataSource
    {
        #region Configuration-properties

        private const string AttrKey = "Attribute";
        private const string FilterKey = "Value";
        private const string LangKey = "Language";
        private const string OperatorKey = "Operator";

        //private const string AllowedValueOperators = "=,<,>,===,contains";
        //private const string AllowedRelationshipOperators = "=,===,contains";

	    private string[] OperatorsForNumber = "=,===,>,<".Split(',');
        private string[] OperatorsForString = "=,===,contains".Split(',');

		/// <summary>
		/// The attribute whoose value will be filtered
		/// </summary>
		public string Attribute
		{
			get { return Configuration[AttrKey]; }
			set { Configuration[AttrKey] = value; }
		}

		/// <summary>
		/// The filter that will be used - for example "Daniel" when looking for an entity w/the value Daniel
		/// </summary>
		public string Value
		{
			get { return Configuration[FilterKey]; }
			set { Configuration[FilterKey] = value; }
		}

		/// <summary>
		/// Language to filter for. At the moment it is not used, or it is trying to find "any"
		/// </summary>
		public string Languages
		{
			get { return Configuration[LangKey]; }
			set { Configuration[LangKey] = value; }
		}
		public string Operator
		{
			get { return Configuration[OperatorKey]; }
			set { Configuration[OperatorKey] = value; }
		}
		#endregion

		/// <summary>
		/// Constructs a new ValueFilter
		/// </summary>
		public ValueFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetEntities));
			Configuration.Add(AttrKey, "[Settings:Attribute]");
			Configuration.Add(FilterKey, "[Settings:Value]");
            Configuration.Add(OperatorKey, "[Settings:Operator||==]");
			Configuration.Add(LangKey, "Default"); // "[Settings:Language|Any]"); // use setting, but by default, expect "any"

            CacheRelevantConfigurations = new[] { AttrKey, FilterKey, LangKey };
        }

		private IEnumerable<IEntity> GetEntities()
		{
			// todo: maybe do something about languages?
			EnsureConfigurationIsLoaded();

			var attr = Attribute;
			// var filter = Value;

            #region do language checks and finish initialization
			var lang = Languages.ToLower();
            if(lang != "default")
				throw  new Exception("Can't filter for languages other than 'default'");
            if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");
		    _initializedLangs = new[] { lang };
            #endregion

            var originals = In[Constants.DefaultStreamName].LightList;

            #region stop if the list is empty
            if (!originals.Any()) 
		        return originals;
            #endregion; 

            var firstEntity = originals.FirstOrDefault(x => x.Attributes.ContainsKey(attr));
            
            // if I can't find any, return empty list
		    if (firstEntity == null)
		        return new List<IEntity>();

            var attribute = firstEntity[attr];
		    var typeName = attribute.Type;  // must get from the property, as not all content-types have a real type object


            _initializedFilter = Value;
		    Func<IEntity, bool> compare;// = StringIsEqual;
            switch (typeName)
            {
                case "Boolean": // todo: find some constant for this
                    compare = GetBoolComparison(Value);
                    break;
                case "Number":
                    compare = GetNumberComparison(Value);
                    break;
                case "DateTime":
                    compare = GetDateTimeComparison(Value);
                    break;
                case "String":
                default:
                    compare = GetStringComparison();
                    break;
            }

            _initializedAttrName = attr;

            // do type checks

		    return GetFilteredWithLinq(originals, compare);
		    //_results = GetFilteredWithLoop(originals, compare);
		}

        #region String Comparison

        private Func<IEntity, bool> GetStringComparison()
        {
            var operation = Operator.ToLower();
            if (operation == "===") // case sensitive, full equal
                StringCompare = StringIsIdentical;

            if (operation == "==")
                StringCompare = StringIsComparable;

            if (operation == "!=")
                StringCompare = StringIsNotComparable;

            if (operation == "contains")
                StringCompare = StringContains;

            if (operation == "!contains")
                StringCompare = StringDoesntContain;

            if (operation == "begins")
                StringCompare = StringBegins;

            if (StringCompare != null)
                return StringGetAndCompare;
            throw new Exception("Wrong operator for string compare, can't find comparison for '" + operation + "'");
        }

        private Func<object, bool> StringCompare; 
        private bool StringGetAndCompare(IEntity e)
            => StringCompare(e.GetBestValue(_initializedAttrName, _initializedLangs));

        private bool StringIsIdentical(object value) 
            => value != null && value.ToString() == _initializedFilter;

        private bool StringIsComparable(object value) 
            => value != null && string.Equals(value.ToString(), _initializedFilter, StringComparison.InvariantCultureIgnoreCase);

        private bool StringIsNotComparable(object value) 
            => value != null && !string.Equals(value.ToString(), _initializedFilter, StringComparison.InvariantCultureIgnoreCase);

        private bool StringContains(object value)
            => value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) > -1;

        private bool StringBegins(object value)
            => value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) == 0;

        private bool StringDoesntContain(object value)
            => value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) == -1;
        #endregion

        #region boolean comparisons
        private bool _boolFilter;
        private Func<IEntity, bool> GetBoolComparison(string original)
        {
            _boolFilter = bool.Parse(Value);

            string operation = Operator.ToLower();
            if (operation == "==" || operation == "===")
                return BoolIsEqual;
            if (operation == "!=")
                return BoolIsNotEqual;

            throw new Exception("Wrong operator for boolean compare, can't find comparison for '" + operation + "'");
        }
        private bool BoolIsEqual(IEntity e)
        {
            var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
            return value as bool? == _boolFilter;
        }

        private bool BoolIsNotEqual(IEntity e)
        {
            var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
            return value as bool? != _boolFilter;
        }

        #endregion

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

        #region date-time comparisons


        private Func<IEntity, bool> GetDateTimeComparison(string original)
        {
            var operation = Operator.ToLower();
            DateTime maxDateTime = DateTime.MaxValue,
                referenceDateTime = DateTime.MinValue;
            if (operation == "between" || operation == "!between")
            {
                var parts = BetweenParts(original);
                if (parts.Item1)
                {
                    DateTime.TryParse(parts.Item2, out referenceDateTime);
                    DateTime.TryParse(parts.Item3, out maxDateTime);
                }
                else
                    operation = "==";
            }

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
                {"between", value => value >= referenceDateTime && value <= maxDateTime },
                {"!between", value => !(value >= referenceDateTime && value <= maxDateTime) },
            };

            if(!dateComparisons.ContainsKey(operation))
                throw new Exception("Wrong operator for datetime compare, can't find comparison for '" + operation + "'");

            DateTimeCompare = dateComparisons[operation];
            return DateTimeGetAndCompare;
        }

        private Func<DateTime, bool> DateTimeCompare;

        private bool DateTimeGetAndCompare(IEntity e)
        {
            var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
            if (value == null)
                return false;
            try
            {
                var valAsDec = Convert.ToDateTime(value);
                return DateTimeCompare(valAsDec);
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region number comparison

        private decimal _numberFilter;
        private Func<IEntity, bool> GetNumberComparison(string original)
        {
            var operation = Operator.ToLower();

            decimal max = decimal.MaxValue;
            _numberFilter = decimal.MinValue;
            if (operation == "between" || operation == "!between")
            {
                var parts = BetweenParts(original);
                if (parts.Item1)
                {
                    decimal.TryParse(parts.Item2, out _numberFilter);
                    decimal.TryParse(parts.Item3, out max);
                }
                else
                    operation = "==";
            }

            if (_numberFilter == decimal.MinValue)
                decimal.TryParse(original, out _numberFilter);

            var numComparisons = new Dictionary<string, Func<decimal, bool>>()
            {
                {"==", value => value == _numberFilter},
                {"===", value => value == _numberFilter},
                {"!=", value => value != _numberFilter},
                {">", value => value > _numberFilter},
                {"<", value => value < _numberFilter},
                {">=", value => value >= _numberFilter},
                {"<=", value => value <= _numberFilter},
                {"between", value => value >= _numberFilter && value <= max },
                {"!between", value => !(value >= _numberFilter && value <= max) },
            };

            if(!numComparisons.ContainsKey(operation))
                throw new Exception("Wrong operator for number compare, can't find comparison for '" + operation + "'");

            NumberCompare = numComparisons[operation];
            return NumberGetAndCompare;
            //if (operation == "===")
            //    NumberCompare = NumberIsEqual;

            //if (operation == "==")
            //    NumberCompare = NumberIsEqual;

            //if (operation == "!=")
            //    NumberCompare = NumberIsNotEqual;

            //if (operation == ">")
            //    NumberCompare = NumberIsGreater;

            //if (operation == "<")
            //    NumberCompare = NumberIsSmaller;

            //if (operation == ">=")
            //    NumberCompare = NumberIsGreaterOrEq;

            //if (operation == "<=")
            //    NumberCompare = NumberIsSmallerOrEq;

            // if (NumberCompare != null)
            //    return NumberGetAndCompare;

            //throw new Exception("Wrong operator for number compare, can't find comparison for '" + operation + "'");
        }

        private Func<decimal, bool> NumberCompare;

        private bool NumberGetAndCompare(IEntity e)
        {
            var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
            if (value == null)
                return false;
            try
            {
                var valAsDec = Convert.ToDecimal(value);
                return NumberCompare(valAsDec);
            }
            catch
            {
                return false;
            }
        }

        //private bool NumberIsEqual(decimal value)
        //    => value == _numberFilter;
        //private bool NumberIsNotEqual(decimal value)
        //    => value != _numberFilter;

        //private bool NumberIsGreater(decimal value)
        //    => value > _numberFilter;

        //private bool NumberIsSmaller(decimal value)
        //    => value < _numberFilter;

        //private bool NumberIsGreaterOrEq(decimal value)
        //    => value >= _numberFilter;

        //private bool NumberIsSmallerOrEq(decimal value)
        //    => value <= _numberFilter;

        #endregion

        //private const string NullError = "{error: not found}";
        private string _initializedFilter;
	    private string[] _initializedLangs;
	    private string _initializedAttrName;

	    private IEnumerable<IEntity> GetFilteredWithLinq(IEnumerable<IEntity> originals, Func<IEntity, bool> compare)//, string attr, string lang)//, string filter)
	    {
            try
	        {
	            var results = (from e in originals
	                where (compare(e)) //) e.GetBestValue(_initializedAttrName, langArr) ?? NullError).ToString() == _initializedFilter
                               select e);
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