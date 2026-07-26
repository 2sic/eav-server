using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Fields.Sys;

/// <summary>
/// Model to read general properties of a field.
/// </summary>
/// <remarks>
/// * Added v22
/// * Not yet tested/verified
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSpecs(ContentType = IFieldSettingsGeneral.Constants.ContentTypeName)]
internal record FieldSettingsGeneralModel : ModelFromEntityBasic, IFieldSettingsGeneral
{
    public string Name => GetThis<string>("");
    public string DefaultValue => GetThis<string>("");
    public string Notes => GetThis<string>("");
    public string InputType => GetThis<string>("");
    public bool Disabled => GetThis(false);
    public bool Required => GetThis(false);
    public bool VisibleInEditUi => GetThis(true);
    public bool DisableTranslation => GetThis(false);
    public bool? DisableAutoTranslation => GetThis<bool?>(null);
    public string Placeholder => GetThis<string>("");
    public bool? IsEphemeral => GetThis<bool?>(null);
    public bool? IsUnique => GetThis<bool?>(null);
    public object Formulas => GetThis<object>(null);
}