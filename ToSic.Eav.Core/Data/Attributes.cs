using System.Text.RegularExpressions;

namespace ToSic.Eav.Data;

/// <summary>
/// Constants related to attributes.
/// Moved here from Eav.Constants
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Attributes
{
    #region Special Virtual-Attribute Names

    public const string IsPublishedField = "IsPublished";
    public const string PublishedEntityField = "PublishedEntity";
    public const string RepoIdInternalField = "_RepositoryId";
    public const string DraftEntityField = "DraftEntity";

    #endregion

    #region System field names - how they are shown / used publicly

    /// <summary>
    /// This is for public values / fields - like when the title is streamed in an API
    /// </summary>
    public const string TitleNiceName = "Title";

    public const string NameIdNiceName = "NameId";

    public const string ModifiedNiceName = "Modified";
    public const string CreatedNiceName = "Created";

    public const string IdNiceName = "Id";
    public const string GuidNiceName = "Guid";

    public static Dictionary<string, string> SystemFields = new()
    {
        { IdNiceName, NumberNiceName },
        { GuidNiceName, nameof(Guid) },
        { CreatedNiceName, nameof(DateTime) },
        { ModifiedNiceName, nameof(DateTime) },
        { TitleNiceName, nameof(String) },
    };
    public static Dictionary<string, string> SystemFieldDescriptions = new()
    {
        { IdNiceName, "ID (int) of this Entity" },
        { GuidNiceName, "GUID (guid) of this Entity" },
        { CreatedNiceName, "When this Entity was created" },
        { ModifiedNiceName, "When this Entity was modified" },
        { TitleNiceName, "Nice title, typically used in dropdowns" },
    };

    public const string TargetNiceName = "Target";
    public const string NumberNiceName = "Number";
    public const string StringNiceName = "String";

    public const string JsonKeyMetadataFor = "For";
    public const string JsonKeyMetadata = "Metadata";

    #endregion


    #region Special Attributes / Fields of Entities in lower-case

    public const string EntityFieldTitle = "entitytitle";
    public const string EntityFieldId = "entityid";
    public const string EntityFieldAutoSelect = "entity-title-id"; // Special code used in a data-source to auto-check title or id
    public const string EntityFieldGuid = "entityguid";
    public const string EntityFieldType = "entitytype";
    public const string EntityFieldIsPublished = "ispublished";
    public const string EntityFieldCreated = "created";
    public const string EntityFieldModified = "modified";
    public const string EntityFieldOwner = "owner";
    public const string EntityFieldOwnerId = "ownerid";
    public const string EntityFieldCreatorWIP = "creator";  // this is not in use yet, but probably will be soon

    public const string EntityIdPascalCase = "EntityId";
    public const string EntityGuidPascalCase = "EntityGuid";

    /// <summary>
    /// New v15.04
    /// </summary>
    public const string EntityAppId = "appid";

    #endregion

    /// <summary>
    /// Reserved field names - the UI should prevent creating fields with this name
    /// </summary>
    public static Dictionary<string, string> ReservedNames { get; } = new()
    {
        { EntityFieldTitle, "This is a unique name for the entity title."},
        { EntityFieldId, "This is a unique name for the entity Id."},
        { EntityFieldGuid, "This is a unique name for the entity Guid."},
        { EntityFieldType, "This is a unique name for the entity type name."},
        { EntityFieldIsPublished, "This is a property which tells the system if the entity is published (and not draft)."},
        { EntityFieldCreated, "This is an internal field which tells us when the entity was created."},
        { EntityFieldModified, "This is an internal field which tells us when the entity was last modified."},
        { EntityFieldOwner, "This is used for the property of the owner of the Entity "},
        { EntityFieldOwnerId, "This is used for the property of the owner of the Entity "},
        { EntityFieldCreatorWIP, "This is used for the property of the creator of the Entity "},
        { "for", "This is an internal information which tells us if the entity is metadata for something."},
        { "metadata", "This is usually a property on the entity which tells us about additional metadata of this entity."},
        { "toolbar", "This is used as a property to generate a toolbar (in DNN only)" },

        { "count", "This is a real property on IDynamicObject so you shouldn't use it" },
        { "entity", "This is a very common term in 2sxc, and would confuse users. " },
        { "id", "This could easily be confused with the ID of an entity, so you shouldn't use it. Prefer ProductId or something." },
        { "guid", "This is a very common term in 2sxc and will usually return the Entity Guid, so you shouldn't create a field with the same name" },
        { "type", "This can easily be confused with the EntityType." },
        { "presentation", "This is a common property term in 2sxc." },

        { "field", "This can easily be mistaken for the Field() method on dynamic entities"},
    };

    /// <summary>
    /// Determine if a field-name is an internal/special property. For ValueFilter and ValueSort
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static (bool isSpecial, ValueTypes fieldType) InternalOnlyIsSpecialEntityProperty(string name)
    {
        return name.ToLowerInvariant() switch
        {
            EntityFieldTitle => (true, ValueTypes.String),
            EntityFieldId => (true, ValueTypes.Number),
            EntityFieldGuid => (true, ValueTypes.Undefined),
            EntityFieldType => (true, ValueTypes.Undefined),
            EntityFieldIsPublished => (true, ValueTypes.Boolean),
            EntityFieldCreated => (true, ValueTypes.DateTime),
            EntityFieldModified => (true, ValueTypes.DateTime),
            _ => (false, ValueTypes.Undefined)
        };
    }

    #region DB Field / Names Constants

    /// <summary>
    /// AttributeSet StaticName must match this Regex. Accept Alphanumeric, except the first char must be alphabetic or underscore.
    /// </summary>
    public static Regex StaticNameValidation =
        new("^[_a-zA-Z]{1}[_a-zA-Z0-9]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// If AttributeSet StaticName doesn't match, users see this message.
    /// </summary>
    public static string StaticNameErrorMessage = "Only alphanumerics and underscore is allowed, first char must be alphabetic or underscore.";

    #endregion
}