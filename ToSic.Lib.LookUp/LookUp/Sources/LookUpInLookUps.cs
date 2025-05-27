namespace ToSic.Lib.LookUp.Sources;

/// <summary>
/// This Value Provider chains two or more LookUps and tries one after another to deliver a result
/// It's mainly used to override values which are given, by a special situation. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
/// <param name="name">Name to use - if stored in a list</param>
/// <param name="providers">list of providers</param>
[PublicApi]
public class LookUpInLookUps(string name, IEnumerable<ILookUp> providers) : LookUpBase(name)
{
    [PrivateApi]
    public List<ILookUp> Providers = [.. providers];

    public override string Description => $"Lookup in multiple lookups: {string.Join(",", Providers.Select(p => p?.Name))}";
        
    /// <inheritdoc/>
    public override string Get(string key, string format)
    {
        var usedSource = Providers.FirstOrDefault(p => !string.IsNullOrEmpty(p.Get(key)));
        return usedSource == null ? string.Empty : usedSource.Get(key, format);
    }

}