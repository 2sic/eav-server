using static ToSic.Eav.Data.Attributes;

namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// Quick helper to get $select to work properly, and force-replace the title
/// Should probably be modified so these aspects are placed in the rules, to reduce edge cases
/// </summary>
internal class SelectSpecs
{
    public SelectSpecs(List<string> selectFields)
    {
        if (selectFields == null) return;
        ApplySelect = selectFields.Any();

        // Force adding the title as it's a special case
        // First check for "EntityTitle" - as that would mean we should add it, but rename it
        var dropNames = SystemFields.Keys.ToList();
        if (selectFields.Any(sf => sf.EqualsInsensitive(EntityFieldTitle)))
        {
            ForceAddTitle = true;
            CustomTitleFieldName = "EntityTitle"; // can't 
            dropNames.Add(EntityFieldTitle);

            // if we have both EntityTitle and Title, we should NOT drop the Title Nice name
            // Note that this is case-safe, since the original list comes from the system-fields dictionary
            dropNames.Remove(TitleNiceName);
        }
        else
        {
            ForceAddTitle = selectFields.Any(sf => sf.EqualsInsensitive(TitleNiceName));
        }

        // Simpler fields, these do not have a custom name feature, they are either added or not
        AddId = selectFields.Any(sf => sf.EqualsInsensitive(IdNiceName));
        AddGuid = selectFields.Any(sf => sf.EqualsInsensitive(GuidNiceName));
        AddModified = selectFields.Any(sf => sf.EqualsInsensitive(ModifiedNiceName));
        AddCreated = selectFields.Any(sf => sf.EqualsInsensitive(CreatedNiceName));

        // drop all system fields
        SelectFields = selectFields.Where(sf => dropNames.Any(key => !key.EqualsInsensitive(sf))).ToList();
    }

    /// <summary>
    /// Should we apply this select-filter on the final fields?
    /// </summary>
    public bool ApplySelect { get; }


    public List<string> SelectFields { get; }

    /// <summary>
    /// Force add the title - optionally (see below) on a different attribute name
    /// </summary>
    public bool ForceAddTitle { get; }

    /// <summary>
    /// When adding the title, optionally override the field name
    /// </summary>
    public string CustomTitleFieldName { get; }


    public bool? AddId { get; }
    public bool? AddGuid { get; }
    public bool? AddModified { get; }
    public bool? AddCreated { get; }
}