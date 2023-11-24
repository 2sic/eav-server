// ReSharper disable NotAccessedField.Global
namespace ToSic.Eav.WebApi.Dto;

public class AppExportInfoDto
{
    public string Name;
    public string Guid;
    public string Version;
    public int EntitiesCount;
    public int LanguagesCount;
    public int TemplatesCount;
    public bool HasRazorTemplates;
    public bool HasTokenTemplates;
    public int FilesCount;
    public int TransferableFilesCount;
}