using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.ImportExport.Sys;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportApp(ExportHelper exportHelper) : ServiceBase("Bck.Export", connect: [exportHelper])
{
    public PathCasePreflightResult PathCasePreflight(
        AppExportSpecs specs,
        Func<(IEnumerable<PathCaseItem> App, IEnumerable<PathCaseItem> Shared)>? getReferences = null)
    {
        var l = Log.Fn<PathCasePreflightResult>(specs.Dump());
        var (_, zipExport) = exportHelper.GetZipExportAndCheckZoneSwitchPermissions(specs);
        var (appReferences, sharedAppReferences) = ReferencesOrEmpty(getReferences);
        var result = zipExport.PathCasePreflight(specs, appReferences, sharedAppReferences);
        return l.Return(result, $"{result.Issues.Count} issues");
    }

    public FileToUploadToClient Export(
        AppExportSpecs specs,
        Func<(IEnumerable<PathCaseItem> App, IEnumerable<PathCaseItem> Shared)>? getReferences = null)
    {
        var l = Log.Fn<FileToUploadToClient>(specs.Dump());

        var (appReader, zipExport) = exportHelper.GetZipExportAndCheckZoneSwitchPermissions(specs);

        var addOnWhenContainingContent = specs.IncludeContentGroups
            ? "_withPageContent"
            : "";

        var fileName =
            $"2sxcApp{appReader.Specs.ToFileNameWithVersion()}{addOnWhenContainingContent}_{DateTime.Now:yyyy-MM-ddTHHmm}.zip";
        l.A($"file name:{fileName}");

        var (appReferences, sharedAppReferences) = ReferencesOrEmpty(getReferences);
        using var fileStream = zipExport.ExportApp(specs, appReferences, sharedAppReferences);
        var fileBytes = fileStream.ToArray();

        return l.Return(new()
        {
            FileName = fileName,
            ContentType = MimeTypeConstants.FallbackType,
            FileBytes = fileBytes
        }, $"will stream so many bytes: {fileBytes.Length}");
    }

    private static (IEnumerable<PathCaseItem> App, IEnumerable<PathCaseItem> Shared) ReferencesOrEmpty(
        Func<(IEnumerable<PathCaseItem> App, IEnumerable<PathCaseItem> Shared)>? getReferences)
        => getReferences?.Invoke()
            ?? (Enumerable.Empty<PathCaseItem>(), Enumerable.Empty<PathCaseItem>());
}
