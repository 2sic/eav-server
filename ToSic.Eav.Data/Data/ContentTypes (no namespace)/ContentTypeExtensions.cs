using ToSic.Eav.Data.ContentTypes.Fields;

namespace ToSic.Eav.Data;

/// <summary>
/// Extension methods for data-related operations.
/// </summary>
/// <remarks>
/// Introduced in v21.
/// To use this, make sure you have `using ToSic.Eav.Data;` in your file.
/// </remarks>
[PublicApi]
public static partial class ContentTypeExtensions
{
    /// <summary>
    /// Detect if the fieldDef is of Boolean type.
    /// </summary>
    public static bool IsBoolean(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.Boolean;

    /// <summary>
    /// Detect if the fieldDef is of DateTime type.
    /// </summary>
    public static bool IsDateTime(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.DateTime;

    /// <summary>
    /// Detect if the fieldDef is of Entity type.
    /// </summary>
    public static bool IsEntity(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.Entity;

    /// <summary>
    /// Detect if the fieldDef is of Hyperlink type.
    /// </summary>
    public static bool IsHyperlink(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.Hyperlink;

    /// <summary>
    /// Detect if the fieldDef is of Number type.
    /// </summary>
    public static bool IsNumber(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.Number;

    /// <summary>
    /// Detect if the fieldDef is of String type.
    /// </summary>
    public static bool IsString(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.String;

    /// <summary>
    /// Detect if the fieldDef is of Empty type.
    /// This means it won't store any data and is mainly used for grouping or messages.
    /// </summary>
    public static bool IsEmpty(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.Empty;

    /// <summary>
    /// Detect if the fieldDef is of Custom type.
    /// This is mainly used for GPS coordinates.
    /// </summary>
    public static bool IsCustom(this IContentTypeField fieldDef) => fieldDef.Type == ValueTypes.Custom;

    /// <summary>
    /// Empty fields can be group titles.
    /// </summary>
    public static bool IsGroupTitle(this IContentTypeField fieldDef) => fieldDef.InputType.StartsWith("empty-default");

    /// <summary>
    /// Empty fields can mark the end of a group.
    /// </summary>
    public static bool IsGroupEnd(this IContentTypeField fieldDef) => fieldDef.InputType.StartsWith("empty-end");

    /// <summary>
    /// Empty fields can be used as messages.
    /// </summary>
    /// <returns></returns>
    public static bool IsMessage(this IContentTypeField fieldDef) => fieldDef.InputType.StartsWith("empty-message");

    /// <summary>
    /// Ephemeral attributes are not stored in the database and are mainly used for formulas or temporary data.
    /// </summary>
    /// <param name="fieldDef"></param>
    /// <returns></returns>
    /// <remarks>
    /// Ephemeral attributes are not stored in the database and are mainly used for formulas or temporary data.
    /// 
    /// Added a bit later in v21.08.
    /// </remarks>
    public static bool IsEphemeral(this IContentTypeField fieldDef) =>
        fieldDef.Metadata.Get<bool>(nameof(IFieldSettingsGeneral.IsEphemeral), typeName: IFieldSettingsGeneral.Constants.ContentTypeName);
}
