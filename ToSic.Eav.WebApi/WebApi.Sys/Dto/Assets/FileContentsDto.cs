namespace ToSic.Eav.WebApi.Sys.Dto;

/// <summary>
/// helper class, because it's really hard to get a post-body in a web-api call if it's not in a json-object format
/// </summary>
public class FileContentsDto
{
    public string Content;
}