using ToSic.Eav.Apps.Sys;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public record ContextDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextAppDto? App { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextLanguageInfoDto? Language { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextUserDto? User { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextResourceWithApp? System { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextResourceWithApp? Site { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public WebResourceDto? Page { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public ContextEnableDto? Enable { get; init; }


    [JsonIgnore(Condition = WhenWritingNull)]
    public IList<FeatureDto>? Features { get; init; }
}

public record WebResourceDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public int? Id { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Url { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? SharedUrl { get; init; }
}

public record ContextResourceWithApp: WebResourceDto
{
    [JsonIgnore(Condition = WhenWritingNull)]
    public AppIdentityPure? DefaultApp { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)]
    public AppIdentityPure? PrimaryApp { get; init; }
}