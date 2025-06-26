namespace ToSic.Eav.WebApi.Sys.Dto;

public record ContentTypeFieldMetadataDto
{
    // temp - goal would be to have many such possible extensions, and the UI would know what to do with them
    [JsonPropertyName("isRecommended")]
    public required bool IsRecommended { get; init; }

    [JsonPropertyName("entityId")]
    public required int EntityId { get; init; }

    [JsonPropertyName("typeName")]
    public required string TypeName { get; init; }
}