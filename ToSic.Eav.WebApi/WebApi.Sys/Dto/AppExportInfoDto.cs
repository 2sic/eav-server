// ReSharper disable NotAccessedField.Global
namespace ToSic.Eav.WebApi.Sys.Dto;

public class AppExportInfoDto
{
    public required string Name { get; init; }
    public required string Guid { get; init; }
    public required string Version { get; init; }
    public required int EntitiesCount { get; init; }
    public required int LanguagesCount { get; init; }
    public required int TemplatesCount { get; init; }
    public required bool HasRazorTemplates { get; init; }
    public required bool HasTokenTemplates { get; init; }
    public required int FilesCount { get; init; }
    public required int TransferableFilesCount { get; init; }
}