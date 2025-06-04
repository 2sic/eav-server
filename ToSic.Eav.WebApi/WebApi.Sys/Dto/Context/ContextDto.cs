using ToSic.Eav.WebApi.Context;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.WebApi.Dto;

public class ContextDto
{
    [JsonIgnore(Condition = WhenWritingNull)] public ContextAppDto App { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public ContextLanguageDto Language { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public ContextUserDto User { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public ContextResourceWithApp System { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public ContextResourceWithApp Site { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public WebResourceDto Page { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public ContextEnableDto Enable { get; set; }

    [JsonIgnore(Condition = WhenWritingNull)] public IList<FeatureDto> Features { get; set; }
}

public class WebResourceDto
{
    [JsonIgnore(Condition = WhenWritingNull)] public int? Id { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public string Url { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public string SharedUrl { get; set; }
}

public class ContextResourceWithApp: WebResourceDto
{
    [JsonIgnore(Condition = WhenWritingNull)] public AppIdentity DefaultApp { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)] public AppIdentity PrimaryApp { get; set; }
}