using ToSic.Eav.ImportExport.Sys.Options;

namespace ToSic.Eav.ImportExport.Sys;

/// <summary>
/// Import content data package, containing the file with additional info which is specific to content.
/// </summary>
public class ContentFileImportOptions
{
    public int AppId { get; init; }

    public required string DefaultLanguage { get; init; }

    public ImportResolveReferenceMode ImportResourcesReferences { get; init; }

    public ImportDeleteUnmentionedItems ClearEntities { get; init; }

    public required string ContentType { get; init; }

    public required string ContentBase64 { get; init; }

    public string DebugInfo =>
        $"app:{AppId} + deflang:{DefaultLanguage}, + ct:{ContentType} + base64:(not shown), impRes:{ImportResourcesReferences}, clear:{ClearEntities}";
}