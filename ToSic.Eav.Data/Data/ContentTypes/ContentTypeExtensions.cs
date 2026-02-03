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
    /// Detect if the attribute is of Boolean type.
    /// </summary>
    public static bool IsBoolean(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.Boolean;

    /// <summary>
    /// Detect if the attribute is of DateTime type.
    /// </summary>
    public static bool IsDateTime(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.DateTime;

    /// <summary>
    /// Detect if the attribute is of Entity type.
    /// </summary>
    public static bool IsEntity(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.Entity;

    /// <summary>
    /// Detect if the attribute is of Hyperlink type.
    /// </summary>
    public static bool IsHyperlink(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.Hyperlink;

    /// <summary>
    /// Detect if the attribute is of Number type.
    /// </summary>
    public static bool IsNumber(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.Number;

    /// <summary>
    /// Detect if the attribute is of String type.
    /// </summary>
    public static bool IsString(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.String;

    /// <summary>
    /// Detect if the attribute is of Empty type.
    /// This means it won't store any data and is mainly used for grouping or messages.
    /// </summary>
    public static bool IsEmpty(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.Empty;

    /// <summary>
    /// Detect if the attribute is of Custom type.
    /// This is mainly used for GPS coordinates.
    /// </summary>
    public static bool IsCustom(this IContentTypeAttribute attribute) => attribute.Type == ValueTypes.Custom;

    /// <summary>
    /// Empty fields can be group titles.
    /// </summary>
    public static bool IsGroupTitle(this IContentTypeAttribute attribute) => attribute.InputType.StartsWith("empty-default");

    /// <summary>
    /// Empty fields can mark the end of a group.
    /// </summary>
    public static bool IsGroupEnd(this IContentTypeAttribute attribute) => attribute.InputType.StartsWith("empty-end");

    /// <summary>
    /// Empty fields can be used as messages.
    /// </summary>
    /// <returns></returns>
    public static bool IsMessage(this IContentTypeAttribute attribute) => attribute.InputType.StartsWith("empty-message");
}
