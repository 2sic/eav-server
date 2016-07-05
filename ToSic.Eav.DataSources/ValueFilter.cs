using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            Configuration.Add(OperatorKey, "[Settings:Operator||=]");
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
                    _boolFilter = bool.Parse(Value);
                    compare = GetBoolComparison();
                    break;
                case "Number":
                    //break;
                case "String":
                default:
                    compare = GetStringComparison();
                    break;
            }

            _initializedAttrName = attr;

            // do type checks

		    return GetFilteredWithLinq(originals, compare); //, attr, lang);//, filter);
		    //_results = GetFilteredWithLoop(originals, attr, lang, filter);
		}

        #region String Comparison

        private Func<IEntity, bool> GetStringComparison()
        {
            var operation = Operator;
            if (operation == "===") // case sensitive, full equal
                StringCompare = StringIsIdentical;

            if (operation == "=")
                StringCompare = StringIsComparable;

            if (operation == "contains")
                StringCompare = StringContains;

            if (operation == "begins")
                StringCompare = StringBegins;

            if (StringCompare != null)
                return StringGetAndCompare;
            throw new Exception("Wrong operator for string compare, can't find comparison for '" + operation + "'");
        }

        private Func<object, bool> StringCompare = null; 
        private bool StringGetAndCompare(IEntity e)
            => StringCompare(e.GetBestValue(_initializedAttrName, _initializedLangs));

        private bool StringIsIdentical(object value) 
            => value != null && value.ToString() == _initializedFilter;
        private bool StringIsComparable(object value) 
            => value != null && string.Equals(value.ToString(), _initializedFilter, StringComparison.InvariantCultureIgnoreCase);

        private bool StringContains(object value)
            => value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) > -1;

        private bool StringBegins(object value)
            => value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) == 0;
     //   private bool StringIsIdentical(IEntity e)
	    //{
     //       var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
	    //    return value != null && value.ToString() == _initializedFilter;
	    //}
     //   private bool StringIsComparable(IEntity e)
	    //{
     //       var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
     //       return value != null && string.Equals(value.ToString(), _initializedFilter, StringComparison.InvariantCultureIgnoreCase);
	    //}

        //private bool StringContains(IEntity e)
        //{
        //    var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
        //    return value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) > -1;
        //}

        //private bool StringBegins(IEntity e)
        //{
        //    var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
        //    return value?.ToString().IndexOf(_initializedFilter, StringComparison.OrdinalIgnoreCase) == 0;
        //}
        #endregion

        #region boolean comparisons
        private Func<IEntity, bool> GetBoolComparison()
        {
            string operation = Operator;
            if (operation == "=" || operation == "===")
                return BoolIsEqual;
            throw new Exception("Wrong operator for boolean compare, can't find comparison for '" + operation + "'");
        }
        private bool BoolIsEqual(IEntity e)
        {
            var value = e.GetBestValue(_initializedAttrName, _initializedLangs);
            return value as bool? == _boolFilter;
        }

        #endregion

        //private const string NullError = "{error: not found}";
        private string _initializedFilter;
	    private bool _boolFilter;
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