namespace ToSic.Eav.WebApi.Dto;

/// <summary>
/// Will be enhanced later
/// </summary>
public class ContextUserDto
{
    public string Email { get; set; }
    public Guid Guid { get; set; }
    public int Id { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsSystemAdmin { get; set; }

    public bool IsSiteAdmin { get; set; }
    public bool IsContentAdmin { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }

}