using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Data.ContentTypes.Fields;

/// <summary>
/// Raw data for Field Settings - but only minimal attributes which we'll usually generate
/// </summary>
/// <remarks>
/// Does not implement <see cref="IFieldSettingsGeneral"/>
/// as that has many more fields we don't care about in most create-data scenarios.
/// </remarks>
[ContentTypeUse(Type = typeof(IFieldSettingsGeneral))]
internal record FieldSettingsGeneralMinimal: IRawEntityAutoConvert
{
    public string Notes { get; init; } = "";

    public string InputType { get; init; } = "";

    internal static FieldSettingsGeneralMinimal? FromCodeAttributeOrNull(ContentTypeFieldAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Notes = attr.Description ?? "",
                InputType = attr.InputTypeWIP ?? ""
            };
}
