namespace ToSic.Eav.Data;

/// <summary>
/// Value / Attribute Type List
/// </summary>
[PublicApi]
public enum ValueTypes
{
    /// <summary> Used for unknown cases, where you would otherwise use null </summary>
    Undefined,  // note: must be first! this is important, otherwise certain code will break as the first value is the "default" / null-value


    /// <summary>Boolean Value Type</summary>
    Boolean,
    /// <summary>DateTime Value Type</summary>
    DateTime,
    /// <summary>Entity Value Type</summary>
    Entity,
    /// <summary>Hyperlink Value Type</summary>
    Hyperlink,
    /// <summary>Number Value Type</summary>
    Number,
    /// <summary>String Value Type</summary>
    String,

    /// <summary>Empty for titles etc. </summary>
    Empty,

    /// <summary> Custom data-type - for custom serialization, will store string </summary>
    Custom,

    /// <summary>
    /// Json data-type, will usually have various help to ensure that dynamic code is optimized
    /// </summary>
    /// <remarks>
    /// Experimentally added in 12.05, but not fully useful yet, as the DB must also be updated
    /// </remarks>
    Json

}