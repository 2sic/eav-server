// ReSharper disable NotAccessedField.Global

using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Apps.ImportExport.Sys;

[ContentType(
    Name = "AppStatistics",
    Guid = "16753307-5d96-4ad8-adc3-a23b2c41edca",
    Description = "App Statistics Information",
    Scope = "System"
)]
public record AppExportInfoRaw: /*RawEntity,*/ IRawEntityAutoConvert
{
    public int Id { get; init; }

    [ContentTypeTitle]
    public required string Name { get; init; }

    public required string NameId { get; init; }
    
    public required string Version { get; init; }
    public required int EntitiesCount { get; init; }
    public required int LanguagesCount { get; init; }
    public required int TemplatesCount { get; init; }
    public required bool HasRazorTemplates { get; init; }
    public required bool HasTokenTemplates { get; init; }
    public required int FilesCount { get; init; }
    public required int TransferableFilesCount { get; init; }
}