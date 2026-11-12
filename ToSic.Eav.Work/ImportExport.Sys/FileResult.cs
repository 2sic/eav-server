namespace ToSic.Eav.ImportExport.Sys;

public record FileResult
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] FileBytes { get; init; }
}
