using ToSic.Lib.Coding;
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

    private Dictionary<string, ILookUp> SourceDic { get; } = new(InvariantCultureIgnoreCase);

    /// <inheritdoc/>
    public IEnumerable<ILookUp> Sources => SourceDic.Values;

    /// <summary>
    /// List of all Configurations for this DataSource
    /// </summary>
    private readonly TokenReplace _reusableTokenReplace;

    public LookUpEngine(ILog parentLog, NoParamOrder protector = default, IEnumerable<ILookUp> sources = default) : base(parentLog, "EAV.LookUp")
    {
        _reusableTokenReplace = new(this);
        if (sources != null)
            Add(sources);
    }

    /// <summary>
    /// Cloning another LookUpEngine and keep the sources.
    /// </summary>
    public LookUpEngine(
        ILookUpEngine original,
        ILog parentLog,
        NoParamOrder protector = default,
        List<ILookUp> sources = default,
        List<ILookUp> overrides = default,
        bool skipOriginalSource = false,
        bool onlyUseProperties = false
        ): this(parentLog)
    {
        if (original == null) return;
        var l = Log.Fn($"clone: {original.Log.NameId}; LogDetailed: {LogDetailed}; {nameof(onlyUseProperties)}: {onlyUseProperties}");
        
        // Link downstream. This is either the original, or if we copy the sources, then we won't use the original, so we use its downstream
        Link(onlyUseProperties ? original.Downstream : original);

        // If we should make our own copy of the sources, we'll copy them all, and link the downstream of the original
        if (onlyUseProperties && !skipOriginalSource)
            Add(original.Sources);

        if (sources != null)
            Add(sources);

        if (overrides != null)
            AddOverride(overrides);

        l.Done($"cloned {original.Sources.Count()}; added {sources?.Count}; overrides: {overrides?.Count}");
    }

    public ILookUpEngine Downstream { get; private set; }

    public ILookUp FindSource(string name) => SourceDic.ContainsKey(name)
        ? SourceDic[name]
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

    public DicString LookUp(DicString values, IEnumerable<ILookUp> overrides, int depth = 4)
    {
        var overridesList = overrides?.ToList() ?? [];
        var l = Log.Fn<DicString>($"values: {values.Count}, overrides: {overridesList.Count()}, depth: {depth}");
        // start by creating a copy of the dictionary
        values = new Dictionary<string, string>(values, InvariantCultureIgnoreCase);

        if (values.Count == 0)
            return l.Return(values, "no values");

        // if there are instance-specific additional Property-Access objects, add them to the sources-list
        // note: it's important to create a one-time use list of sources if instance-specific sources are needed, to never modify the "global" list.
        if (!overridesList.Any())
            return l.ReturnAsOk(LookUp(values, depth));

        var innerLookup = new LookUpEngine(this, Log, sources: [.. overridesList]);
        return l.ReturnAsOk(innerLookup.LookUp(values, depth));
    }

    // 2024-05-06 2dm
    ///// <inheritdoc />
    //private void Add(ILookUp lookUp)
    //{
    //    Log.A(LogDetailed, $"Add/replace source: '{lookUp.Name}'");
    //    Sources[lookUp.Name] = lookUp;
    //}

    private void Add(IEnumerable<ILookUp> lookUps)
    {
        if (lookUps == null) return;
        var list = lookUps.ToList();
        if (list.Count == 0) return;
        var sourceNames = Log.Try(() => string.Join(", ", list.Select(l => $"'{l.Name ?? "unknown"}'")));
        Log.A(Log.Try(() => $"Add/replace sources: {sourceNames}"));
        foreach (var lookUp in list) 
            SourceDic[lookUp.Name] = lookUp;
    }

    ///// <summary>
    ///// Add an overriding source. <br/>
    ///// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
    ///// </summary>
    ///// <param name="lookUp">a <see cref="ILookUp"/> which should override the original configuration</param>
    private void AddOverride(ILookUp lookUp)
    {
        var found = SourceDic.TryGetValue(lookUp.Name, out var original);
        var l = (LogDetailed ? Log : null).Fn($"{lookUp.Name}; found existing: {found}");
        SourceDic[lookUp.Name] = found
            ? new LookUpInLookUps(lookUp.Name, lookUp, original)
            : lookUp;
        l.Done();
    }

    /// <summary>
    /// Add many overriding sources. <br/>
    /// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
    /// </summary>
    /// <param name="lookUps">list of <see cref="ILookUp"/> which should override the original configuration</param>
    private void AddOverride(IEnumerable<ILookUp> lookUps)
    {
        if (lookUps == null) return;
        foreach (var provider in lookUps)
            if (provider.Name == null)
                throw new NullReferenceException("LookUp must have a Name");
            else
                // check if it already has this provider. 
                // ensure that there is an "override property provider" which would pre-catch certain keys
                AddOverride(provider);
    }
}