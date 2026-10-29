using static System.StringComparer;

namespace ToSic.Sys.Logging;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LogStoreEntry
{
    /// <summary>
    /// Special title - beginning with space, so it will be on top of the A-Z list.
    /// </summary>
    public const string TitleKey = " Title";

    public ILog? Log { get; internal set; }

    public IDictionary<string, string>? Specs { get; internal set; }

    internal Action<IReadOnlyDictionary<string, string>>? SpecsChanged { private get; set; }

    public void AddSpec(string key, string value)
    {
        Specs ??= new Dictionary<string, string>(InvariantCultureIgnoreCase);
        var changed = !Specs.TryGetValue(key, out var previous) || previous != value;
        Specs[key] = value;
        if (changed)
            NotifySpecsChanged();
    }

    public void UpdateSpecs(IDictionary<string, string>? specs)
    {
        // Skip if nothing new
        if (specs == null || specs.Count == 0)
            return;

        // If we have no specs yet, just take the new ones
        if (Specs == null || Specs.Count == 0)
        {
            Specs = specs;
            NotifySpecsChanged();
            return;
        }

        // Merge specs
        var changed = false;
        foreach (var pair in specs)
        {
            var pairChanged = !Specs.TryGetValue(pair.Key, out var previous) || previous != pair.Value;
            Specs[pair.Key] = pair.Value;
            changed |= pairChanged;
        }
        if (changed)
            NotifySpecsChanged();
    }

    /// <summary>
    /// Optional "better" title for insights
    /// </summary>
    public string? Title => Specs?.TryGetValue(TitleKey, out var title) == true
        ? title
        : null;

    private void NotifySpecsChanged()
    {
        if (SpecsChanged == null || Specs == null)
            return;
        // Send a copy because callers may continue changing Specs after this callback.
        SpecsChanged(new Dictionary<string, string>(Specs, InvariantCultureIgnoreCase));
    }
}
