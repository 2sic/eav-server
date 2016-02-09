using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ToSic.Eav.DataSources.Exceptions;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Return only Entities having a specific value in an Attribute
	/// </summary>
	[PipelineDesigner]
	public class ValueFilter : BaseDataSource
	{
		#region Configuration-properties

		private const string AttrKey = "Attribute";
		private const string FilterKey = "Value";
		private const string LangKey = "Language";


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
		#endregion

		/// <summary>
		/// Constructs a new ValueFilter
		/// </summary>
		public ValueFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetEntities));
			Configuration.Add(AttrKey, "[Settings:Attribute]");
			Configuration.Add(FilterKey, "[Settings:Value]");
			Configuration.Add(LangKey, "Default"); // "[Settings:Language|Any]"); // use setting, but by default, expect "any"

            CacheRelevantConfigurations = new[] { AttrKey, FilterKey, LangKey };
        }

		private IEnumerable<IEntity> GetEntities()
		{
			// todo: maybe do something about languages?

			EnsureConfigurationIsLoaded();
			var attr = Attribute;
			var filter = Value;
			var lang = Languages.ToLower();
			if(lang != "default")
				throw  new Exception("Can't filter for languages other than 'default'");

			var originals = In[Constants.DefaultStreamName].LightList;

			//if (string.IsNullOrEmpty(Value) && PassThroughOnEmptyValue)
			//	return originals;


			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");

            return GetFilteredWithLinq(originals, attr, lang, filter);
            //_results = GetFilteredWithLoop(originals, attr, lang, filter);
		}


	    private IEnumerable<IEntity> GetFilteredWithLinq(IEnumerable<IEntity> originals, string attr, string lang, string filter)
	    {
	        var langArr = new string[] {lang};
	        string nullError = "{error: not found}";
            try
	        {
	            var results = (from e in originals
	                where (e.GetBestValue(attr, langArr) ?? nullError).ToString() == filter
	                select e);
	            return results;//.ToDictionary(x => x.Key, y => y.Value);
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
        /// <returns></returns>
	    private IDictionary<int, IEntity> GetFilteredWithLoop(IDictionary<int, IEntity> inList, string attr, string lang, string filter)
	    {
            var result = new Dictionary<int, IEntity>();
            var langArr = new string[] { lang };
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