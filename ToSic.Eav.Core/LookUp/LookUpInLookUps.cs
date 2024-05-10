using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp;

/// <summary>
/// This Value Provider chains two or more LookUps and tries one after another to deliver a result
/// It's mainly used to override values which are given, by a special situation. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[PublicApi]
public class LookUpInLookUps: LookUpBase
{
    [PrivateApi]
    public List<ILookUp> Providers = [];

    private string _description;

    /// <summary>
    /// Generate a lookup-of-lookups. 
    /// </summary>
    /// <param name="name">Name to use - if stored in a list</param>
    /// <param name="first">First LookUp source</param>
    /// <param name="second">Second LookUp source</param>
    /// <param name="third">Optional third</param>
    /// <param name="fourth">Optional fourth</param>
    public LookUpInLookUps(string name, ILookUp first, ILookUp second = null, ILookUp third = null, ILookUp fourth = null) : base(name)
    {
        Providers.Add(first);
        if (second != null) Providers.Add(second);
        if (third != null) Providers.Add(third);
        if (fourth != null) Providers.Add(fourth);
    }

    public override string Description => $"Lookup in multiple lookups: {string.Join(",", Providers.Select(p => p?.Name))}";

    // not sure - doesn't seem used?
    [PrivateApi]
    public LookUpInLookUps(string name, List<ILookUp> providers) : base(name)
    {
        Providers = providers;
    }
        
        
    /// <inheritdoc/>
    public override string Get(string key, string format)
    {
        var usedSource = Providers.FirstOrDefault(p => !string.IsNullOrEmpty(p.Get(key)));
        return usedSource == null ? string.Empty : usedSource.Get(key, format);
    }

}