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

        // Force adding the title below, as it's a special case
        ForceAddTitle = selectFields.Any(sf => sf.EqualsInsensitive(Attributes.TitleNiceName));
        AddId = selectFields.Any(sf => sf.EqualsInsensitive(Attributes.IdNiceName));
        AddGuid = selectFields.Any(sf => sf.EqualsInsensitive(Attributes.GuidNiceName));
        AddModified = selectFields.Any(sf => sf.EqualsInsensitive(Attributes.ModifiedNiceName));
        AddCreated = selectFields.Any(sf => sf.EqualsInsensitive(Attributes.CreatedNiceName));

        // drop all system fields
        SelectFields = selectFields.Where(sf => Attributes.SystemFields.Keys.Any(key => !key.EqualsInsensitive(sf))).ToList();
    }
    public bool ApplySelect { get; }
    public List<string> SelectFields { get; }
    public bool ForceAddTitle { get; }
    public bool? AddId { get; }
    public bool? AddGuid { get; }
    public bool? AddModified { get; }
    public bool? AddCreated { get; }
}