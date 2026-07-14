using System.Collections;

namespace ToSic.Eav.Data.Raw.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RawFromAnonymousHelper(ILog parentLog): HelperBase(parentLog, "Raw.FrAnon")
{
    public IRawEntity Convert(object? original)
    {
        var l = Log.Fn<IRawEntity>();

        // if original is null, assume empty anonymous
        original ??= new { };

        var dic = original.ToDicInvariantInsensitive(mutable: true);
        
        var basic = new RawEntityRecord
        {
            Id = ExtractConvert<int>(nameof(IRawEntity.Id)),
            Guid = ExtractConvert<Guid>(nameof(IRawEntity.Guid)),
            Created = ExtractConvert<DateTime>(nameof(IRawEntity.Created)),
            Modified = ExtractConvert<DateTime>(nameof(IRawEntity.Modified)),
            Values = null!,   // this is just temp
            RelationshipKeys = null!, // this is just temp
        };

        var extractKeys = ExtractRelationshipKeys(basic.Id, dic);
        
        var typedRelationships = StrongTypeRelationships(extractKeys.values);

        basic = basic with
        {
            Values = typedRelationships,
            RelationshipKeys = extractKeys.relationshipKeys
        };

        return l.Return(basic);

        // Helper to extract a value and remove it from the original dictionary, converting it to the specified type
        TVal? ExtractConvert<TVal>(string name)
        {
            if (!dic.TryGetValue(name, out var value))
                return default;
            dic.Remove(name);
            return value.ConvertOrDefault<TVal>();
        }
    }

    /// <summary>
    /// Extract any additional relationship keys from the dictionary, and return them along with the remaining values.
    /// If no additional keys are found, return the ID as the only relationship key.
    /// </summary>
    /// <param name="id">The main ID which will always be listed as a relationship key.</param>
    /// <param name="dic">The dictionary of values to extract relationship keys from.</param>
    /// <returns>A tuple containing the remaining values and the list of relationship keys.</returns>
    internal (IDictionary<string, object?> values, IList<object> relationshipKeys) ExtractRelationshipKeys(int id, IDictionary<string, object?> dic)
    {
        var l = Log.Fn<(IDictionary<string, object?> values, IList<object> relationshipKeys)>($"{id}");

        const string relKeysField = nameof(IHasRelationshipKeys.RelationshipKeys);
        
        // Start with keys containing the ID, as this is a key for establishing relationships, and will be used if no other keys are found
        IList<object> relationshipsDefault = [id];

        // Check if the dictionary of properties has any relationship keys (must be a list), and if so, use them in addition to the ID
        if (!dic.TryGetValue(relKeysField, out var maybeRels) ||
            maybeRels is not (IEnumerable rels and not string))
            return l.Return((dic, relationshipsDefault), "no additional keys");
        
        try
        {
            l.A("Found relationship keys");
            var relationshipKeys = rels.Cast<object>().ToList();
            relationshipKeys.Add(id);
            // only remove if no exception occured - so it stays in if something is wrong, making it easier to spot issues
            dic.Remove(relKeysField);
            return l.Return((dic, relationshipKeys));
        }
        catch
        {
            // Silent info, no throw
            return l.Return((dic, relationshipsDefault), $"Error in {nameof(RawFromAnonymousHelper)} trying to convert {relKeysField}");
        }

    }

    /// <summary>
    /// Strongly type any relationships found in the dictionary of values, replacing them with typed RawRelationship objects.
    /// </summary>
    /// <param name="dic">The dictionary of values to process.</param>
    /// <returns>A dictionary with strongly typed relationships.</returns>
    internal IDictionary<string, object?> StrongTypeRelationships(IDictionary<string, object?> dic)
    {
        var l = Log.Fn<IDictionary<string, object?>>();
        var replacements = ExtractRelationships(dic);
        if (replacements.Count == 0)
            return l.Return(dic, "no relationships");

        var updated = new Dictionary<string, object?>(dic);
        foreach (var replacement in replacements)
            updated[replacement.Key] = replacement.Value;

        return l.Return(updated, $"Replaced {replacements.Count} relationships");

    }
    
    /// <summary>
    /// Extract all objects / properties which contain relationships, and correctly type them.
    /// </summary>
    /// <param name="dic">The dictionary of values to extract relationships from.</param>
    /// <returns>A dictionary of typed relationships.</returns>
    internal IDictionary<string, RawRelationship> ExtractRelationships(IDictionary<string, object?> dic)
    {
        var l = Log.Fn<IDictionary<string, RawRelationship>>();

        // Scan relationships in values dictionary
        var replacements = dic
            // Skip if not an anonymous object (which is how relationships are passed in)
            .Where(pair => pair.Value.IsAnonymous())
            .Select(pair => new
            {
                pair.Key,
                // Try to parse and see if it has a "Relationships" property, which is how relationships are passed in
                // otherwise skip this key, as it is not a relationship
                relsTemp = pair.Value!.ObjectToDictionary(caseInsensitive: false)
                    .TryGetValue(RawRelationship.RelationshipsKey, out var relsTemp)
                    ? relsTemp
                    : null
            })
            .Where(pair => pair.relsTemp != null)
            .ToDictionary(
                pair => pair.Key,
                pair => new RawRelationship(
                    // If we only have on object / string, use it as key
                    // if we have an IEnumerable use the list as the keys
                    keys: pair.relsTemp is IEnumerable relsList and not string
                        ? relsList.Cast<object>().ToListOpt()
                        : [pair.relsTemp]
                )
            );

        return l.Return(replacements, $"Typed {replacements.Count} relationships");
    }
}
