using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Takes a list of configuration masks (list of tokens) and resolves them with a bunch of LookUps. <br/>
    /// Read more about this in @Specs.LookUp.Intro
    /// </summary>
    [PublicApi]
    public class LookUpEngine : HasLog, ILookUpEngine
	{
        #region Constants

        [PrivateApi] public const int DefaultLookUpDepth = 4;
        [PrivateApi] protected new bool LogDetailed = true;
        #endregion

        // todo: probably change and not let the outside modify directly
        [PrivateApi]
	    public Dictionary<string, ILookUp> Sources { get; }
	        = new Dictionary<string, ILookUp>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of all Configurations for this DataSource
        /// </summary>
        private readonly TokenReplace _reusableTokenReplace;

		///// <summary>
		///// Constructs a new LookUpEngine
		///// </summary>
		//public LookUpEngine(): this(null as ILog) { }

        public LookUpEngine(ILog parentLog): base("EAV.LookUp", parentLog)
		{
			_reusableTokenReplace = new TokenReplace(Sources);
		}

        /// <inheritdoc />
        /// <summary>
        /// Cloning another LookUpEngine and keep the sources.
        /// BUT: Don't keep the overrides, as these will be unique in the clone. 
        /// </summary>
        public LookUpEngine(ILookUpEngine original, ILog parentLog): this(parentLog)
		{
		    if (original == null) return;
            var wrapLog = Log.Call(null, $"clone: {original.Log.Id}; LogDetailed: {LogDetailed}");
		    foreach (var srcSet in original.Sources)
		        Sources.Add(srcSet.Key, srcSet.Value);
            wrapLog($"cloned {original.Sources.Count}");
        }

	    /// <inheritdoc />
	    public IDictionary<string, string> LookUp(IDictionary<string, string> values,
            IDictionary<string, ILookUp> overrides = null,
            int depth = 4)
        {
            var wrapLog = Log.Call($"values: {values.Count}, overrides: {overrides?.Count}, depth: {depth}");
            // start by creating a copy of the dictionary
            values = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);

            if (values.Count == 0)
            {
                wrapLog("no values");
                return values;
            }

            #region if there are instance-specific additional Property-Access objects, add them to the sources-list
            // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
            var useAdditionalPa = overrides != null; // not null, so it has instance specific stuff
		    if (useAdditionalPa)
            {
                var added = "";
                var skipped = "";
                foreach (var pa in Sources)
                    if (!overrides.ContainsKey(pa.Key))
                    {
                        if (LogDetailed) added += pa.Key + ",";
                        overrides.Add(pa.Key.ToLower(), pa.Value);
                    }
                    else if (LogDetailed) skipped += pa.Key + ",";

                if (LogDetailed)
                    Log.Add($"skipped original [{skipped}] " +
                            $"and added originals [{added}]");
            }

            var instanceTokenReplace = useAdditionalPa ? new TokenReplace(overrides) : _reusableTokenReplace;
            #endregion

            #region Loop through all config-items and token-replace them
            foreach (var o in values.ToList())
			{
                // check if the string contains a token or not
                if (!TokenReplace.ContainsTokens(o.Value))
                {
                    if (LogDetailed) Log.Add($"token '{o.Key}={o.Value}' has no sub-tokens");
                    continue;
                }

                var result = instanceTokenReplace.ReplaceTokens(o.Value, depth); // with 2 further recurrences
                if (LogDetailed) Log.Add($"token '{o.Key}={o.Value}' is now '{result}'");
                values[o.Key] = result;
            }
            #endregion

            wrapLog("ok");
            return values;
        }

        // 2019-11-07 2dm doesn't seem used
	    //public string Replace(string sourceText) => _reusableTokenReplace.ReplaceTokens(sourceText);


        /// <inheritdoc />
	    public void Add(ILookUp lookUp)
        {
            if (LogDetailed) Log.Add($"Add source: '{lookUp.Name}'");
            Sources[lookUp.Name] = lookUp;
        }

        /// <inheritdoc />
        public void AddOverride(ILookUp lookUp)
        {
            var wrapLog = LogDetailed ? Log.Call() : null;
	        if (Sources.ContainsKey(lookUp.Name))
	            Sources[lookUp.Name] =
	                new LookUpInLookUps(lookUp.Name, lookUp,
	                    Sources[lookUp.Name]);
	        else
	            Sources.Add(lookUp.Name, lookUp);
            wrapLog?.Invoke("ok");
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