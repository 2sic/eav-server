using ToSic.Eav.Data.ContentTypes.Fields.Sys;
using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Fields;

/// <summary>
/// Content-Type for the general settings of a field (attribute) on a content-type.
/// </summary>
/// <remarks>
/// Note that as of 2026-07-26 there is no model yet to use, but it should be added soon.
/// </remarks>
[ModelSpecs(ContentType = Constants.ContentTypeName)]
[ContentType(
    Name = Constants.ContentTypeName,
    Guid = "0bab4be8-e795-4d9f-b50e-f7ec161ed8cb",  // made-up GUID, can't match DB Guid since it's currently `@All`
    Description = "General settings for every Attribute (field) on a Content-Type."
)]
public interface IFieldSettingsGeneral : IModelFromEntity<FieldSettingsGeneralModel>
{
    [PrivateApi]
    public static class Constants { public const string ContentTypeName = "@All"; }

    [ContentTypeField(IsTitle = true)]
    string Name { get; }

    string DefaultValue { get; }

    /// <summary>
    /// Description of this field.
    /// </summary>
    string Notes { get; }

    /// <summary>
    /// The official input-type - usually something like `@string-default`
    /// </summary>
    string InputType { get; }

    //ValidationRegExJavaScript

    //Warnings

    //Errors

    bool Disabled { get; }

    bool Required { get; }

    bool VisibleInEditUi { get; }

    bool DisableTranslation { get; }

    bool? DisableAutoTranslation { get; }

    string Placeholder { get; }

    /// <summary>
    /// Determines if this field is ephemeral, meaning it is not stored in the database and only exists temporarily during processing.
    /// </summary>
    bool? IsEphemeral { get; }

    bool? IsUnique { get; }

    /// <summary>
    /// The formulas associated with this field, which can be used for calculations or transformations.
    /// </summary>
    [ContentTypeField(Type = ValueTypes.Entity)]
    object? Formulas { get; }

}