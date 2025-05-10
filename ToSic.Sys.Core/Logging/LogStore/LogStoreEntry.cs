using System.Collections.Generic;
using static System.StringComparer;

namespace ToSic.Lib.Logging;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogStoreEntry
{
    /// <summary>
    /// Special title - beginning with space, so it will be on top of the A-Z list.
    /// </summary>
    public const string TitleKey = " Title";

    public ILog? Log { get; internal set; }

    public IDictionary<string, string>? Specs { get; internal set; }

    public void AddSpec(string key, string value)
    {
        Specs ??= new Dictionary<string, string>(InvariantCultureIgnoreCase);
        Specs[key] = value;
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
            return;
        }

        // Merge specs
        foreach (var pair in specs)
            AddSpec(pair.Key, pair.Value);
    }

    /// <summary>
    /// Optional "better" title for insights
    /// </summary>
    public string? Title => Specs?.TryGetValue(TitleKey, out var title) == true
        ? title
        : null;
}