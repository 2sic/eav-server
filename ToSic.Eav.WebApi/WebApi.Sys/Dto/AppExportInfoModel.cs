// ReSharper disable NotAccessedField.Global

using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class AppExportInfoModel: RawEntity
{
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

    public override IDictionary<string, object?> Attributes(RawConvertOptions options) =>
        new Dictionary<string, object?>
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