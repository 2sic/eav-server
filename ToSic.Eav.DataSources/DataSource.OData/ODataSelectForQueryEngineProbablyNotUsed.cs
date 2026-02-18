using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.DataSource.OData;

/// <summary>
/// Apply Select clause to a set of entities
/// </summary>
/// <remarks>
/// WARNING: This is called, but it appears that the result is never used!!!
/// </remarks>
public sealed class ODataSelectForQueryEngineProbablyNotUsed
{
    public ODataSelectForQueryEngineProbablyNotUsed(ICollection<string>? select)
    {
        // Some fields may have paths, e.g. "Author/Name"
        // So we first take them apart, to see if any have multiple paths
        var fieldsPathArray = (select ?? [])
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.Trim().Split('/'))
            // Make sure we only keep non-empty root paths, so nothing like "/Name"
            .Where(f => !string.IsNullOrWhiteSpace(f[0]))
            .ToArray();

        // The fields we keep are only the first part, non-empty, distinct
        Fields = fieldsPathArray
            .Select(f => f[0])
            // Distinct now, because there could be "Author/Name" and "Author/Email" both leading to "Author"
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Subfields = fieldsPathArray
            // Only keep those with more than one part
            .Where(f => f.Length > 1)
            // Group by first part
            .GroupBy(f => f[0], StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                // Get the first part of the path
                group => group.Key,
                // Keep / join all the remaining parts
                group => group
                    .Select(field => string.Join("/", field.Skip(1)))
                    .ToList(),
                StringComparer.OrdinalIgnoreCase
            );
    }


    /// <summary>
    /// Get a clean list of fields
    /// </summary>
    private List<string> Fields { get; }

    private Dictionary<string, List<string>> Subfields { get; }

    private Dictionary<string, ODataSelectForQueryEngineProbablyNotUsed> SubSelects => field
        ??= Subfields.ToDictionary(
            kvp => kvp.Key,
            kvp => new ODataSelectForQueryEngineProbablyNotUsed(kvp.Value),
            StringComparer.OrdinalIgnoreCase
        );

    public IReadOnlyList<IDictionary<string, object?>> ApplySelect(IEnumerable<IEntity> entities)
    {
        // Check if no fields specified so we only do this once
        var noFields = Fields.Count == 0;

        // Now project each entity into a dictionary of the selected fields
        var result = entities
            .Select(entity =>
                noFields
                    // If no fields specified, return all attributes
                    ? entity.Attributes.Keys.ToDictionary(
                        attribute => attribute,
                        entity.Get,
                        StringComparer.OrdinalIgnoreCase
                    )
                    // Otherwise return only the selected fields
                    : Fields.ToDictionary(
                        field => field,
                        field => GetValueAndProcessSubSelects(entity, field),
                        StringComparer.OrdinalIgnoreCase
                    )
            )
            .ToList();

        return result;
    }

    private object? GetValueAndProcessSubSelects(IEntity entity, string field)
    {
        var result = GetProjectionValue(entity, field);
        return result switch
        {
            null => null,
            IEnumerable<IEntity> asEntities when SubSelects.TryGetValue(field, out var subSelect)
                => subSelect.ApplySelect(asEntities),
            _ => result
        };
    }

    #region Prepare comparison lists so we only do this once

    private static readonly string[] IdFields =
    [
        AttributeNames.EntityFieldId,
        AttributeNames.IdNiceName.ToLowerInvariant()
    ];

    private static readonly string[] GuidFields =
    [
        AttributeNames.EntityFieldGuid,
        AttributeNames.GuidNiceName.ToLowerInvariant()
    ];

    private static readonly string[] CreatedFields = [
        AttributeNames.EntityFieldCreated,
        AttributeNames.CreatedNiceName.ToLowerInvariant()
    ];

    private static readonly string[] ModifiedFields = [
        AttributeNames.EntityFieldModified,
        AttributeNames.ModifiedNiceName.ToLowerInvariant()
    ];

    private static readonly string[] TitleFields = [
        AttributeNames.EntityFieldTitle,
        AttributeNames.TitleNiceName.ToLowerInvariant()
    ];

    #endregion

    private static object? GetProjectionValue(IEntity entity, string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return null;

        // Exact match should happen first
        // If there is a field "Title" which is not the title, it should still be return first.
        var exactMatch = entity.Get(field);
        if (exactMatch != null)
            return exactMatch;

        var lowered = field.ToLowerInvariant();

        if (IdFields.Contains(lowered))
            return entity.EntityId;

        if (GuidFields.Contains(lowered))
            return entity.EntityGuid;

        if (CreatedFields.Contains(lowered))
            return entity.Created;

        if (ModifiedFields.Contains(lowered))
            return entity.Modified;

        if (TitleFields.Contains(lowered))
            return entity.GetBestTitle();

        return null;
    }

}
