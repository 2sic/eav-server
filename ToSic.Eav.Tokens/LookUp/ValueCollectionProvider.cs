using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Tokens;

namespace ToSic.Eav.ValueProviders
{
	/// <inheritdoc />
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	public class ValueCollectionProvider : IValueCollectionProvider
	{
	    public Dictionary<string, IValueProvider> Sources { get; }
	        = new Dictionary<string, IValueProvider>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of all Configurations for this DataSource
        /// </summary>
        private readonly TokenReplace _reusableTokenReplace;

		/// <summary>
		/// Constructs a new Configuration Provider
		/// </summary>
		public ValueCollectionProvider()
		{
			_reusableTokenReplace = new TokenReplace(Sources);
		}

		/// <inheritdoc />
		/// <summary>
		/// ...cloning an original
		/// </summary>
		public ValueCollectionProvider(IValueCollectionProvider original): this()
		{
		    if (original == null) return;
		    foreach (var srcSet in original.Sources)
		        Sources.Add(srcSet.Key, srcSet.Value);
		}

	    /// <inheritdoc />
	    /// <summary>
	    /// This will go through a dictionary of strings (usually configuration values) and replace all tokens in that string
	    /// with whatever the token-resolver delivers. It's usually needed to initialize a DataSource. 
	    /// </summary>
	    /// <param name="configList">Dictionary of configuration strings</param>
	    /// <param name="instanceSpecificPropertyAccesses">Instance specific additional value-dictionaries</param>
	    /// <param name="repeat">max repeater in case of recursions</param>
	    public void LoadConfiguration(IDictionary<string, string> configList, Dictionary<string, IValueProvider> instanceSpecificPropertyAccesses = null, int repeat = 2)
		{
            #region if there are instance-specific additional Property-Access objects, add them to the sources-list
            // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
            var useAdditionalPa = instanceSpecificPropertyAccesses != null; // not null, so it has instance specific stuff
		    if (useAdditionalPa)
		        foreach (var pa in Sources)
		            if (!instanceSpecificPropertyAccesses.ContainsKey(pa.Key))
		                instanceSpecificPropertyAccesses.Add(pa.Key.ToLower(), pa.Value);
		    var instanceTokenReplace = useAdditionalPa ? new TokenReplace(instanceSpecificPropertyAccesses) : _reusableTokenReplace;
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

	    public string Replace(string sourceText) => _reusableTokenReplace.ReplaceTokens(sourceText);

	    public void Add(IValueProvider newSource) => Sources[newSource.Name] = newSource;

        public void AddOverride(IValueProvider propertyProvider)
	    {
	        if (Sources.ContainsKey(propertyProvider.Name))
	            Sources[propertyProvider.Name] =
	                new OverrideValueProvider(propertyProvider.Name, propertyProvider,
	                    Sources[propertyProvider.Name]);
	        else
	            Sources.Add(propertyProvider.Name, propertyProvider);

        }

	    public void AddOverride(IEnumerable<IValueProvider> providers)
	    {
	        if (providers == null) return;
	        foreach (var provider in providers)

	            if (provider.Name == null)
	                throw new NullReferenceException("PropertyProvider must have a Name");
	            else
	                // check if it already has this provider. 
	                // ensure that there is an "override property provider" which would pre-catch certain keys
	                AddOverride(provider);
	    }
    }
}