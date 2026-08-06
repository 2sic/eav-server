using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

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

// Changes
// 1. record
// 2. primary constructor
// 3. fields in primary constructor
// 4. Attributes in constructor



// old
//public class NameValueRaw : IRawEntityAutoConvert
//{
//    public NameValueRaw(string name, string? value = null)
//    {
//        Name = name;
//        Value = value;
//    }

//    [ContentTypeField(IsTitle = true)]
//    public string Name { get; init; }

//    public string? Value { get; init; }
//}