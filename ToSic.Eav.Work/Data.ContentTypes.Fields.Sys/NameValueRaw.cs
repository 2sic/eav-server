namespace ToSic.Eav.Data.ContentTypes.Fields.Sys;

[ContentType(
    Name = "NameValuePair",
    Guid = "db36e44b-46e1-427c-bd2e-65c84cd5c392",
    Description = "Named system value",
    Scope = "System"
)]
public record NameValueRaw(
    [property: ContentTypeField(IsTitle = true)]
    string Name,
    string? Value = null
) : IRawEntityAutoConvert;
