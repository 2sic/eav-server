namespace ToSic.Eav.Data;

/// <summary>
/// Extends the ValueTypes Enum with additional states such as virtual, notFound etc.
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public enum ValueTypesWithState
{
    Undefined = ValueTypes.Undefined,
    Boolean = ValueTypes.Boolean,
    DateTime = ValueTypes.DateTime,
    Entity = ValueTypes.Entity,
    Hyperlink = ValueTypes.Hyperlink,
    Number = ValueTypes.Number,
    String = ValueTypes.String,
    Empty = ValueTypes.Empty,
    Custom = ValueTypes.Custom,
    Json = ValueTypes.Json,

    /// <summary>
    /// Virtual fields are not real fields, but information properties like title, etc.
    /// </summary>
    Virtual = 101,

    /// <summary>
    /// Fields which never came from an entity but from another object altogether
    /// </summary>
    Dynamic = Virtual + 1,

    Error = 201,

    /// <summary>
    /// Not found - used to mark a get-result when a property is not found
    /// </summary>
    NotFound = Error + 1,


    /// <summary>
    /// Not sure what exactly this is for...
    /// </summary>
    Null = NotFound + 1,
}