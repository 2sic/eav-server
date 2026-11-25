namespace ToSic.Eav.ImportExport.Sys;

/// <summary>
/// Import package - usually transferred from a web client request.
/// </summary>
public record DataFileImportOptions
{
    /// <summary>
    /// Target AppId to import into.
    /// </summary>
    public int AppId { get; init; }

    /// <summary>
    /// Transmitted body/contents, in base64 encoding.
    /// </summary>
    public required string ContentBase64 { get; init; }

}
