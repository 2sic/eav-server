using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ContentType(
    Name = "InputTypeInfo",
    Guid = "8c0b688e-c79a-4180-8123-5d1959f3a89f",
    Description = "Input type information",
    Scope = "System"
)]
public record InputTypeModel(InputTypeInfo inputType) : RawEntity
{
    [ContentTypeField(IsTitle = true)] public string Type => inputType.Type;
    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(InputTypeInfo.Type), inputType.Type }, { nameof(InputTypeInfo.Label), inputType.Label },
        { nameof(InputTypeInfo.Description), inputType.Description }, { nameof(InputTypeInfo.DisableI18n), inputType.DisableI18n },
        { nameof(InputTypeInfo.UiAssets), inputType.UiAssets }, { nameof(InputTypeInfo.UseAdam), inputType.UseAdam },
        { nameof(InputTypeInfo.IsObsolete), inputType.IsObsolete }, { nameof(InputTypeInfo.ObsoleteMessage), inputType.ObsoleteMessage },
        { nameof(InputTypeInfo.IsRecommended), inputType.IsRecommended }, { nameof(InputTypeInfo.IsDefault), inputType.IsDefault },
        { nameof(InputTypeInfo.Source), inputType.Source }, { nameof(InputTypeInfo.ConfigTypes), inputType.ConfigTypes },
    };
}

[ContentType(
    Name = "NameValuePair",
    Guid = "db36e44b-46e1-427c-bd2e-65c84cd5c392",
    Description = "Named system value",
    Scope = "System"
)]
public record NameValueModel(string name, string? value = default) : RawEntity
{
    [ContentTypeField(IsTitle = true)] public string Name => name;
    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(Name), Name }, { "Value", value },
    };
}