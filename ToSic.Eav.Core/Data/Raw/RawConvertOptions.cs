using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using static System.StringComparer;

namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Options which may be needed to create new <see cref="IEntity"/>s from <see cref="IRawEntity"/>.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PublicApi]
public class RawConvertOptions
{
    public RawConvertOptions(
        string noParamOrder = Parameters.Protector,
        IEnumerable<string> addKeys = default
    )
    {
        KeysToAdd = (addKeys ?? Array.Empty<string>())
            .Where(s => s.HasValue())
            .ToImmutableHashSet(InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// List of keys to add.
    /// These are keys which would not be added by default,
    /// either because they take computing resources or because they are often not needed.
    ///
    /// This should happen at the RawEntity-level which generates the dictionary
    /// as it's more efficient.
    /// </summary>
    public ImmutableHashSet<string> KeysToAdd { get; }

    public bool AddKey(string key) => KeysToAdd.Contains(key);
}