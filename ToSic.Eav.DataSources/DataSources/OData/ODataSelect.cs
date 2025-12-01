using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Apply Select clause to a set of entities
/// </summary>
public sealed class ODataSelect
{

    public static IReadOnlyList<IDictionary<string, object?>> ApplySelect(IEnumerable<IEntity> entities, ICollection<string>? select)
    {
        // Get a clean list of fields
        var fields = select?
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Check if no fields specified so we only do this once
        var noFields = fields == null || fields.Count == 0;

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
                    : fields!.ToDictionary(
                        field => field,
                        field => GetProjectionValue(entity, field),
                        StringComparer.OrdinalIgnoreCase
                    )
            )
            .ToList();

        return result;
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
