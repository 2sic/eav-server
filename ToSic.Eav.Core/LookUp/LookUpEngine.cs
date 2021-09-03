using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using DicString = System.Collections.Generic.IDictionary<string, string>;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Takes a list of configuration masks (list of tokens) and resolves them with a bunch of LookUps. <br/>
    /// Read more about this in [](xref:Basics.LookUp.Index)
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi, use Interface in your code ILookupEngine")]
    public class LookUpEngine : HasLog, ILookUpEngine
	{
        #region Constants

        [PrivateApi] public const int DefaultLookUpDepth = 4;
        [PrivateApi] protected new bool LogDetailed = true;
        #endregion

        // todo: probably change and not let the outside modify directly
        [PrivateApi]
	    public Dictionary<string, ILookUp> Sources { get; }
	        = new Dictionary<string, ILookUp>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// List of all Configurations for this DataSource
        /// </summary>
        private readonly TokenReplace _reusableTokenReplace;

		///// <summary>
		///// Constructs a new LookUpEngine
		///// </summary>
		//public LookUpEngine(): this(null as ILog) { }

        public LookUpEngine(ILog parentLog): base("EAV.LookUp", parentLog, "()")
		{
			_reusableTokenReplace = new TokenReplace(this);
		}

        /// <inheritdoc />
        /// <summary>
        /// Cloning another LookUpEngine and keep the sources.
        /// </summary>
        public LookUpEngine(ILookUpEngine original, ILog parentLog, bool makeOwnCopyOfSources = false): this(parentLog)
		{
		    if (original == null) return;
            var wrapLog = Log.Call(null, $"clone: {original.Log.Id}; LogDetailed: {LogDetailed}");
            if (makeOwnCopyOfSources)
            {
                Link(original.Downstream);
                foreach (var srcSet in original.Sources)
                    Sources.Add(srcSet.Key, srcSet.Value);
            }
            else
                Link(original);

            wrapLog($"cloned {original.Sources.Count}");
        }

        [PrivateApi("still wip")]
        public ILookUpEngine Downstream { get; private set; }

        [PrivateApi("still wip")]
        public ILookUp FindSource(string name) => Sources.ContainsKey(name)
            ? Sources[name]
            : Downstream?.FindSource(name);

        [PrivateApi]
        public bool HasSource(string name) => FindSource(name) != null;

        public void Link(ILookUpEngine downstream) => Downstream = downstream;
        
        public DicString LookUp(DicString values, int depth = 4)
        {
            var wrapLog = Log.Call<DicString>($"values: {values.Count}, depth: {depth}");
            // start by creating a copy of the dictionary
            values = new Dictionary<string, string>(values, StringComparer.InvariantCultureIgnoreCase);

            if (values.Count == 0)
                return wrapLog("no values", values);

            var instanceTokenReplace = _reusableTokenReplace;

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

            return wrapLog("ok", values);
        }

        public DicString LookUp(DicString values, IDictionary<string, ILookUp> overrides, int depth = 4)
        {
            var wrapLog = Log.Call<DicString>($"values: {values.Count}, overrides: {overrides?.Count}, depth: {depth}");

            // start by creating a copy of the dictionary
            values = new Dictionary<string, string>(values, StringComparer.InvariantCultureIgnoreCase);

            if (values.Count == 0) return wrapLog("no values", values);

            // if there are instance-specific additional Property-Access objects, add them to the sources-list
            // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
            if (overrides == null || overrides.Count <= 0) return wrapLog("ok", LookUp(values, depth));

            var innerLookup = new LookUpEngine(this, Log);
            foreach (var pa in overrides)
                innerLookup.Sources.Add(pa.Key, pa.Value);
            return wrapLog("ok", innerLookup.LookUp(values, depth));
        }


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