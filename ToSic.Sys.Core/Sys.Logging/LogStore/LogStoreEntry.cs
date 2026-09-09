using static System.StringComparer;

using System.Collections.Immutable;

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

    internal string? Segment { get; init; }

    public IDictionary<string, string>? Specs { get; internal set; }

    public void AddSpec(string key, string value)
    {
        Specs ??= new Dictionary<string, string>(InvariantCultureIgnoreCase);
        Specs[key] = value;
        PublishSpecs();
    }

    public void UpdateSpecs(IDictionary<string, string>? specs)
    {
        // Skip if nothing new
        if (specs == null || specs.Count == 0)
            return;

        // If we have no specs yet, just take the new ones
        if (Specs == null || Specs.Count == 0)
        {
            Specs = new Dictionary<string, string>(specs, InvariantCultureIgnoreCase);
            PublishSpecs();
            return;
        }

        // Merge specs
        foreach (var pair in specs)
            Specs[pair.Key] = pair.Value;
        PublishSpecs();
    }

    internal void PublishSpecs()
    {
        if (Log is Log log && Segment != null && Specs != null)
            LogEventBridge.Write(LogEvent.ForLog(log) with
            {
                Kind = "Specs",
                Segment = Segment,
                Properties = Specs.ToImmutableDictionary(InvariantCultureIgnoreCase),
            });
    }

    /// <summary>
    /// Optional "better" title for insights
    /// </summary>
    public string? Title => Specs?.TryGetValue(TitleKey, out var title) == true
        ? title
        : null;
}
