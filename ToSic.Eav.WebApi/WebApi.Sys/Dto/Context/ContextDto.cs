using ToSic.Eav.Apps.Sys;
using ToSic.Eav.WebApi.Sys.Context;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.WebApi.Sys.Dto;

public record ContextDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextAppDto? App { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextLanguageDto? Language { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextUserDto? User { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextResourceWithApp? System { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextResourceWithApp? Site { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public WebResourceDto? Page { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextEnableDto? Enable { get; set; }


    [JsonIgnore(Condition = WhenWritingNull)]
    public IList<FeatureDto>? Features { get; set; }
}

public record WebResourceDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public int? Id { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Url { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? SharedUrl { get; set; }
}

public record ContextResourceWithApp: WebResourceDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public AppIdentityPure? DefaultApp { get; set; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public AppIdentityPure? PrimaryApp { get; set; }
}