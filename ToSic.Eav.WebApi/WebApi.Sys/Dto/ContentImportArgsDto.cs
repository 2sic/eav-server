using ToSic.Eav.ImportExport.Sys.Options;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class ContentImportArgsDto
{
    public int AppId;

    public required string DefaultLanguage;

    public ImportResolveReferenceMode ImportResourcesReferences;

    public ImportDeleteUnmentionedItems ClearEntities;

    public required string ContentType;

    public required string ContentBase64;

    public string DebugInfo =>
        $"app:{AppId} + deflang:{DefaultLanguage}, + ct:{ContentType} + base64:(not shown), impRes:{ImportResourcesReferences}, clear:{ClearEntities}";
}