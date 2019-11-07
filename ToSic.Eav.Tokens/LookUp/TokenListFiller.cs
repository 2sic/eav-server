using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Tokens;

namespace ToSic.Eav.LookUp
{
	/// <summary>
	/// Takes a list of configuration masks (list of tokens) and resolves them with a bunch of LookUps.
	/// </summary>
	// [PublicApi]
    // TODO: AWAIT CHANGES TO INTERFACE BEFORE WE PUBLISH
	public class TokenListFiller : ITokenListFiller
	{
        // todo: probably change and not let the outside modify directly
        [PrivateApi]
	    public Dictionary<string, ILookUp> Sources { get; }
	        = new Dictionary<string, ILookUp>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of all Configurations for this DataSource
        /// </summary>
        private readonly TokenReplace _reusableTokenReplace;

		/// <summary>
		/// Constructs a new TokenListFiller
		/// </summary>
		public TokenListFiller()
		{
			_reusableTokenReplace = new TokenReplace(Sources);
		}

		/// <inheritdoc />
		/// <summary>
		/// Cloning another TokenListFiller and keep the sources.
		/// BUT: Don't keep the overrides, as these will be unique in the clone. 
		/// </summary>
		public TokenListFiller(ITokenListFiller original): this()
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
	    public void LoadConfiguration(IDictionary<string, string> configList, Dictionary<string, ILookUp> instanceSpecificPropertyAccesses = null, int repeat = 2)
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

        // 2019-11-07 2dm doesn't seem used
	    //public string Replace(string sourceText) => _reusableTokenReplace.ReplaceTokens(sourceText);


        /// <inheritdoc />
	    public void Add(ILookUp lookUp) => Sources[lookUp.Name] = lookUp;

        /// <inheritdoc />
        public void AddOverride(ILookUp lookUp)
	    {
	        if (Sources.ContainsKey(lookUp.Name))
	            Sources[lookUp.Name] =
	                new LookUpInLookUps(lookUp.Name, lookUp,
	                    Sources[lookUp.Name]);
	        else
	            Sources.Add(lookUp.Name, lookUp);

        }

        /// <inheritdoc />
	    public void AddOverride(IEnumerable<ILookUp> lookUps)
	    {
	        if (lookUps == null) return;
	        foreach (var provider in lookUps)

	            if (provider.Name == null)
	                throw new NullReferenceException("PropertyProvider must have a Name");
	            else
	                // check if it already has this provider. 
	                // ensure that there is an "override property provider" which would pre-catch certain keys
	                AddOverride(provider);
	    }
    }
}