using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.XmlList;
using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Users;
using ToSic.Sys.Utils;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportDataXml(IAppsCatalog appsCatalog, Generator<ExportListXml> exportListXmlGenerator)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtEx", connect: [exportListXmlGenerator, appsCatalog])
{
    public FileResult ExportContent(IUser user,
        string language,
        string defaultLanguage,
        string contentType,
        ExportSelection exportSelection,
        ExportResourceReferenceMode exportResourcesReferences,
        ExportLanguageResolution exportLanguageReferences,
        string? selectedIds)
    {
        var l = Log.Fn<FileResult>(
            $"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
        SecurityHelpers.ThrowIfNotContentAdmin(user, l);

        var contextLanguages = appsCatalog.Zone(MyOptions.ZoneId).LanguagesActive
            .Select(lng => lng.EnvironmentKey)
            .ToArray();

        // check if we have an array of ids
        int[]? ids = null;
        try
        {
            if (exportSelection == ExportSelection.Selection && !string.IsNullOrWhiteSpace(selectedIds))
                ids = selectedIds.CsvToArrayWithoutEmpty().Select(int.Parse).ToArray();
        }
        catch (Exception e)
        {
            throw new("trouble finding selected IDs to export", e);
        }

        var tableExporter = exportListXmlGenerator.New().Init(MyOptions.AppReader, contentType);
        var fileContent = exportSelection == ExportSelection.Blank
            ? tableExporter.EmptyListTemplate()!
            : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                exportResourcesReferences == ExportResourceReferenceMode.Resolve, ids!)!;

        var contentTypeName = tableExporter.ContentType.Name;

        var fileName =
            $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
            $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
            $"{DateTime.Now:yyyyMMddHHmmss}.xml";

        return l.ReturnAsOk(new()
        {
            FileName = fileName,
            ContentType = MimeTypeConstants.Xml,
            FileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent)
        });
    }
}
