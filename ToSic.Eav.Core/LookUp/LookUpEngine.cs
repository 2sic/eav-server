using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static System.StringComparer;
using DicString = System.Collections.Generic.IDictionary<string, string>;

namespace ToSic.Eav.LookUp;

/// <summary>
/// Takes a list of configuration masks (list of tokens) and resolves them with a bunch of LookUps. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[PrivateApi("hide implementation")]
public class LookUpEngine : HelperBase, ILookUpEngine
{
    #region Constants

    [PrivateApi] public const int DefaultLookUpDepth = 4;
    [PrivateApi] protected bool LogDetailed = true;
    #endregion

    // todo: probably change and not let the outside modify directly
    [PrivateApi]
    public Dictionary<string, ILookUp> Sources { get; } = new(InvariantCultureIgnoreCase);

    /// <summary>
    /// List of all Configurations for this DataSource
    /// </summary>
    private readonly TokenReplace _reusableTokenReplace;

    public LookUpEngine(ILog parentLog): base(parentLog, "EAV.LookUp")
    {
        _reusableTokenReplace = new(this);
    }

    /// <inheritdoc />
    /// <summary>
    /// Cloning another LookUpEngine and keep the sources.
    /// </summary>
    public LookUpEngine(ILookUpEngine original, ILog parentLog, bool makeOwnCopyOfSources = false): this(parentLog)
    {
        if (original == null) return;
        Log.Do(message: $"clone: {original.Log.NameId}; LogDetailed: {LogDetailed}", action: () =>
        {
            if (makeOwnCopyOfSources)
            {
                Link(original.Downstream);
                foreach (var srcSet in original.Sources)
                    Sources.Add(srcSet.Key, srcSet.Value);
            }
            else
                Link(original);

            return $"cloned {original.Sources.Count}";
        });
    }

    public ILookUpEngine Downstream { get; private set; }

    public ILookUp FindSource(string name) => Sources.ContainsKey(name)
        ? Sources[name]
        : Downstream?.FindSource(name);

    [PrivateApi]
    public bool HasSource(string name) => FindSource(name) != null;

    public void Link(ILookUpEngine downstream) => Downstream = downstream;
        
    public DicString LookUp(DicString values, int depth = 4)
    {
        var l = Log.Fn<DicString>($"values: {values.Count}, depth: {depth}");
        // start by creating a copy of the dictionary
        values = new Dictionary<string, string>(values, InvariantCultureIgnoreCase);

        if (values.Count == 0)
            return l.Return(values, "no values");

        // Loop through all config-items and token-replace them
        foreach (var o in values.ToList())
        {
            // check if the string contains a token or not
            if (!TokenReplace.ContainsTokens(o.Value))
            {
                l.A(LogDetailed, $"token '{o.Key}={o.Value}' has no sub-tokens");
                continue;
            }

            var result = _reusableTokenReplace.ReplaceTokens(o.Value, depth); // with 2 further recurrences
            l.A(LogDetailed, $"token '{o.Key}={o.Value}' is now '{result}'");
            values[o.Key] = result;
        }

        return l.ReturnAsOk(values);
    }

    public DicString LookUp(DicString values, IDictionary<string, ILookUp> overrides, int depth = 4)
    {
        var l = Log.Fn<DicString>($"values: {values.Count}, overrides: {overrides?.Count}, depth: {depth}");
        // start by creating a copy of the dictionary
        values = new Dictionary<string, string>(values, InvariantCultureIgnoreCase);

        if (values.Count == 0)
            return l.Return(values, "no values");

        // if there are instance-specific additional Property-Access objects, add them to the sources-list
        // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
        if (overrides == null || overrides.Count <= 0)
            return l.ReturnAsOk(LookUp(values, depth));

        var innerLookup = new LookUpEngine(this, Log);
        foreach (var pa in overrides)
            innerLookup.Sources.Add(pa.Key, pa.Value);
        return l.ReturnAsOk(innerLookup.LookUp(values, depth));
    }


    /// <inheritdoc />
    public void Add(ILookUp lookUp)
    {
        Log.A(LogDetailed, $"Add/replace source: '{lookUp.Name}'");
        Sources[lookUp.Name] = lookUp;
    }

    public void Add(IList<ILookUp> lookUps)
    {
        var sourceNames = Log.Try(() => string.Join(", ", lookUps.Select(l => $"'{l.Name ?? "unknown"}'")));
        Log.A(Log.Try(() => $"Add/replace sources: {sourceNames}"));
        foreach (var lookUp in lookUps) 
            Sources[lookUp.Name] = lookUp;
    }

    /// <inheritdoc />
    public void AddOverride(ILookUp lookUp) => Log.Do(() =>
    {
        if (Sources.ContainsKey(lookUp.Name))
            Sources[lookUp.Name] = new LookUpInLookUps(lookUp.Name, lookUp, Sources[lookUp.Name]);
        else
            Sources.Add(lookUp.Name, lookUp);
    }, enabled: LogDetailed);

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