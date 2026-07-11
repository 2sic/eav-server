// ReSharper disable NotAccessedField.Global

using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentTypeSpecs(
    Name = "AppStatistics",
    Guid = "16753307-5d96-4ad8-adc3-a23b2c41edca",
    Description = "App Statistics Information",
    Scope = "System"
)]
public class AppExportInfoModel: RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)]
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

    public override IDictionary<string, object?> Values => field ??= new Dictionary<string, object?>
    {
        { nameof(Name), Name },
        { nameof(NameId), NameId },
        { nameof(Version), Version },
        { nameof(EntitiesCount), EntitiesCount },
        { nameof(LanguagesCount), LanguagesCount },
        { nameof(TemplatesCount), TemplatesCount },
        { nameof(HasRazorTemplates), HasRazorTemplates },
        { nameof(HasTokenTemplates), HasTokenTemplates },
        { nameof(FilesCount), FilesCount },
        { nameof(TransferableFilesCount), TransferableFilesCount },
    };
}