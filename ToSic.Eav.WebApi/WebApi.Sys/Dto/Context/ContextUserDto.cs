namespace ToSic.Eav.WebApi.Sys.Dto;

/// <summary>
/// Will be enhanced later
/// </summary>
public class ContextUserDto
{
    public required string Email { get; init; }
    public required Guid Guid { get; init; }
    public required int Id { get; init; }
    public required bool IsAnonymous { get; init; }
    public required bool IsSystemAdmin { get; init; }

    public required bool IsSiteAdmin { get; init; }
    [JsonPropertyName("isContentEditor")]
    public required bool IsContentEditor { get; init; }
    public required bool IsContentAdmin { get; init; }
    public required string Name { get; init; }
    public required string Username { get; init; }

    [JsonPropertyName("roles")]
    public required ICollection<string> Roles { get; init; }
}