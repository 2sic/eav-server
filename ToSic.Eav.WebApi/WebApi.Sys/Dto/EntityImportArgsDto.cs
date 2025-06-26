using System.Text;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class EntityImportDto
{
    public int AppId { get; init; }

    public required string ContentBase64 { get; init; }

    public string DebugInfo => $"app:{AppId} + base:{ContentBase64}";

    public string GetContentString()
    {
        var data = System.Convert.FromBase64String(ContentBase64);
        var str = Encoding.UTF8.GetString(data);
        return str;
    }
}