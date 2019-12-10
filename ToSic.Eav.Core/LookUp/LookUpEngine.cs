using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Takes a list of configuration masks (list of tokens) and resolves them with a bunch of LookUps. <br/>
    /// Read more about this in @Specs.LookUp.Intro
    /// </summary>
    [PublicApi]
    public class LookUpEngine : ILookUpEngine
	{
        #region Constants

        [PrivateApi] public const int DefaultLookUpDepth = 4;
        #endregion

        // todo: probably change and not let the outside modify directly
        [PrivateApi]
	    public Dictionary<string, ILookUp> Sources { get; }
	        = new Dictionary<string, ILookUp>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of all Configurations for this DataSource
        /// </summary>
        private readonly TokenReplace _reusableTokenReplace;

		/// <summary>
		/// Constructs a new LookUpEngine
		/// </summary>
		public LookUpEngine()
		{
			_reusableTokenReplace = new TokenReplace(Sources);
		}

        /// <inheritdoc />
        /// <summary>
        /// Cloning another LookUpEngine and keep the sources.
        /// BUT: Don't keep the overrides, as these will be unique in the clone. 
        /// </summary>
        public LookUpEngine(ILookUpEngine original): this()
		{
		    if (original == null) return;
		    foreach (var srcSet in original.Sources)
		        Sources.Add(srcSet.Key, srcSet.Value);
		}

	    /// <inheritdoc />
	    public IDictionary<string, string> LookUp(IDictionary<string, string> values,
            Dictionary<string, ILookUp> overrides = null, int depth = 4)
		{
            // start by creating a copy of the dictionary
            values = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);

            #region if there are instance-specific additional Property-Access objects, add them to the sources-list
            // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
            var useAdditionalPa = overrides != null; // not null, so it has instance specific stuff
		    if (useAdditionalPa)
		        foreach (var pa in Sources)
		            if (!overrides.ContainsKey(pa.Key))
		                overrides.Add(pa.Key.ToLower(), pa.Value);
		    var instanceTokenReplace = useAdditionalPa ? new TokenReplace(overrides) : _reusableTokenReplace;
            #endregion

            #region Loop through all config-items and token-replace them
            foreach (var o in values.ToList())
			{
                // check if the string contains a token or not
                if (!TokenReplace.ContainsTokens(o.Value))
					continue;
                values[o.Key] = instanceTokenReplace.ReplaceTokens(o.Value, depth); // with 2 further recurrences

            }
            #endregion

            return values;
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