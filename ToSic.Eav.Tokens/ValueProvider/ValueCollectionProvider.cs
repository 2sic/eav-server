using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Tokens;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	public class ValueCollectionProvider : IValueCollectionProvider
	{
		//public string DataSourceType { get; internal set; }
		public Dictionary<string, IValueProvider> Sources { get; }
		/// <summary>
		/// List of all Configurations for this DataSource
		/// </summary>
		//public IDictionary<string, string> configList { get; internal set; }
		private readonly TokenReplace _reusableTokenReplace;

		/// <summary>
		/// Constructs a new Configuration Provider
		/// </summary>
		public ValueCollectionProvider()
		{
			Sources = new Dictionary<string, IValueProvider>(StringComparer.OrdinalIgnoreCase);
			_reusableTokenReplace = new TokenReplace(Sources);
		}

        /// <summary>
        /// This will go through a dictionary of strings (usually configuration values) and replace all tokens in that string
        /// with whatever the token-resolver delivers. It's usually needed to initialize a DataSource. 
        /// </summary>
        /// <param name="configList">Dictionary of configuration strings</param>
        /// <param name="instanceSpecificPropertyAccesses">Instance specific additional value-dictionaries</param>
		public void LoadConfiguration(IDictionary<string, string> configList, Dictionary<string, IValueProvider> instanceSpecificPropertyAccesses = null, int repeat = 2)
		{
            #region if there are instance-specific additional Property-Access objects, add them to the sources-list
            // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
            var useAdditionalPA = (instanceSpecificPropertyAccesses != null); // not null, so it has instance specific stuff
		    if (useAdditionalPA)
		        foreach (var pa in Sources)
		            if (!instanceSpecificPropertyAccesses.ContainsKey(pa.Key))
		                instanceSpecificPropertyAccesses.Add(pa.Key.ToLower(), pa.Value);
		    var instanceTokenReplace = useAdditionalPA ? new TokenReplace(instanceSpecificPropertyAccesses) : _reusableTokenReplace;
            #endregion

            #region Loop through all config-items and token-replace them
            foreach (var o in configList.ToList())
			{
                // check if the string contains a token or not
                if (!TokenReplace.ContainsTokens(o.Value))
					continue;
                configList[o.Key] = instanceTokenReplace.ReplaceTokens(o.Value, repeat); // with 2 further recurrances

            }
            #endregion
        }

	    public string Replace(string sourceText)
	    {
	        return _reusableTokenReplace.ReplaceTokens(sourceText, 0);
	    }
	}
}