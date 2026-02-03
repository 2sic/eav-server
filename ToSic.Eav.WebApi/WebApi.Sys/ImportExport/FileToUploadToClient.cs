namespace ToSic.Eav.WebApi.Sys.ImportExport;

public record FileToUploadToClient
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] FileBytes { get; init; }
}
