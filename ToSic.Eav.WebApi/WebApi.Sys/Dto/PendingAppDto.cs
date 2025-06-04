namespace ToSic.Eav.WebApi.Dto;

public class PendingAppDto
{
    // folder as it's stored on the server
    public string ServerFolder { get; set; }
    // taken from the app.xml
    public string Name { get; set; }
    // taken from the app.xml
    public string Description { get; set; }
    // taken from the app.xml
    public string Version { get; set; }
    // taken from the app.xml
    public string Folder { get; set; }
}