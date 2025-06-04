namespace ToSic.Eav.WebApi.Sys.Dto;

public record ContentTypeFieldMetadataDto
{
    // temp - goal would be to have many such possible extensions, and the UI would know what to do with them
    [JsonPropertyName("isRecommended")]
    public bool IsRecommended { get; set; }

    [JsonPropertyName("entityId")]
    public int EntityId { get; set; }

    [JsonPropertyName("typeName")]
    public string TypeName { get; set; }
}