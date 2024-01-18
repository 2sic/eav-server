using System;
using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogStoreEntry
{
    public ILog Log { get; internal set; }
    public IDictionary<string, string> Specs { get; internal set; }

    public void AddSpec(string key, string value)
    {
        Specs ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        Specs[key] = value;
    }

    public void UpdateSpecs(IDictionary<string, string> specs)
    {
        if (specs == null) return;

        if (Specs == null || specs.Count == 0)
        {
            Specs = specs;
            return;
        }

        // Merge specs
        foreach (var pair in specs) AddSpec(pair.Key, pair.Value);
    }
}