using System.Text.Json;
using static ToSic.Eav.Data.Sys.AttributeNames;

namespace ToSic.Eav.Serialization;

/// <summary>
/// Create a new EntitySerializationDecorator based on the $select parameters in the URL to filter the fields
/// </summary>
/// <param name="rawFields">list of fields to select</param>
/// <param name="withGuid">If it should add the guids by default - usually when the serializer is configured as admin-mode</param>
/// <param name="log">log to use in this static function</param>
/// <returns></returns>
[PrivateApi]
public class EntitySerializationDecoratorCreator(List<string>? rawFields, bool withGuid, ILog log)
{
    public EntitySerializationDecorator Generate(string part = ODataSelect.Main, bool? defaultAdd = false)
    {
        var l = log.Fn<EntitySerializationDecorator>($"{nameof(rawFields)}: {rawFields}, {nameof(withGuid)}: {withGuid}");

        var oDataSelect = new ODataSelect(rawFields);

        // Get fields for this part - or return null if not existing / empty list
        var myFields = oDataSelect.GetFieldsOrNull(part);

        // If nothing, exit. Only specify SerializeGuid if it's true, otherwise null so it can use other defaults
        if (myFields == null)
            return l.Return(new() { SerializeGuid = withGuid ? true : null }, "no filters");

        // Since we seem to have a list of fields, we'll have to filter out system fields
        // So start with a list of all system fields
        var sysFieldsToDrop = SystemFields.Keys.ToList();

        // Prepare to add the title, if it's not in the list
        bool forceAddTitle;

        // If we have a custom title field, we should use that instead of the default title
        string customTitleFieldName = null;

        // Force adding the title as it's a special case
        // First check for "EntityTitle" - as that would mean we should add it, but rename it
        if (myFields.Contains(EntityFieldTitle /* is already lower invariant */))
        {
            l.A($"Found long title name {EntityFieldTitle}, will process replacement strategy");
            forceAddTitle = true;
            customTitleFieldName = "EntityTitle"; // can't 
            sysFieldsToDrop.Add(EntityFieldTitle);

            // if we have both EntityTitle and Title, we should NOT drop the Title Nice name
            // Note that this is case-safe, since the original list comes from the system-fields dictionary
            sysFieldsToDrop.Remove(TitleNiceName);
        }
        else
        {
            forceAddTitle = myFields.Contains(TitleNiceName.ToLowerInvariant());
        }

        // Simpler fields, these do not have a custom name feature, they are either added or not
        var addId = ShouldAdd(IdNiceName) ?? defaultAdd;
        var addGuid = ShouldAdd(GuidNiceName) ?? defaultAdd;
        var addModified = ShouldAdd(ModifiedNiceName) ?? defaultAdd;
        var addCreated = ShouldAdd(CreatedNiceName) ?? defaultAdd;

        // Remove all system field names, as they are handled separately
        var filterFields = myFields
            .Where(sf => !sysFieldsToDrop.Any(key => key.EqualsInsensitive(sf)))
            .ToList();

        var result = new EntitySerializationDecorator
        {
            SerializeId = addId,
            SerializeGuid = addGuid,
            SerializeModified = addModified,
            SerializeCreated = addCreated,
            SerializeTitleForce = forceAddTitle,
            CustomTitleName = customTitleFieldName,
            FilterFieldsEnabled = true,
            FilterFields = filterFields
        };
        return l.Return(result, JsonSerializer.Serialize(result));

        // Helper function to check if the field is in the list
        bool? ShouldAdd(string name)
        {
            var key = name.ToLowerInvariant();
            if (myFields.Contains(key) || myFields.Contains("+" + key))
                return true;
            if (myFields.Contains("-" + key))
                return false;
            return null;
        }
    }

    #region Maybe Someday Feature - prefix could change how fields are loaded


    //public List<string> FieldsMain => field
    //    ??= FieldsCleaned
    //        .Where(f => !f.Contains('.'))
    //        .ToList();

    //public bool MainAddState(string name) => FieldsCleaned.Contains(name.ToLowerInvariant());

    //private const char Exclusive = '~';
    //private const char Add = '+';
    //private const char Remove = '-';

    ///// <summary>
    ///// The first entry (if any) determines the main mode which applies to all further fields
    ///// </summary>
    //public char MainMode = GetMode(rawFields?.FirstOrDefault());

    //internal static char GetMode(string? field)
    //    => field?.FirstOrDefault() switch
    //    {
    //        Add => Add,
    //        Remove => Remove,
    //        _ => Exclusive
    //    };

    #endregion
}