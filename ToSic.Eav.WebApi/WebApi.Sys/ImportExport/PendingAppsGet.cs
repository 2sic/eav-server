using ToSic.Eav.Apps.Sys;
using ToSic.Eav.ImportExport.Sys.ImportHelpers;
using ToSic.Eav.ImportExport.Sys.XmlImport;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Dto;
using ISite = ToSic.Eav.Context.ISite;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class PendingAppsGet(
    AppFinder appFinder,
    ISite site,
    Generator<XmlImportWithFiles> xmlImpExpFiles)
    : ServiceBase("Bck.Export", connect: [appFinder, site, xmlImpExpFiles])
{
    /// <summary>
    /// Get list of pending apps.
    /// List all app folders in the 2sxc which:
    /// - are not installed as apps yet
    /// - have a App_Data/app.xml
    /// </summary>
    public IEnumerable<PendingAppDto> GetPendingApps(int zoneId)
    {
        var l = Log.Fn<IEnumerable<PendingAppDto>>($"list all app folders for zoneId.{zoneId}");
        var result = new List<PendingAppDto>();

        foreach (var directoryPath in Directory.GetDirectories(site.AppsRootPhysicalFull))
        {
            l.A($"find pending app in folder:{directoryPath}");

            var folderName = Path.GetFileName(directoryPath);

            if (appFinder.AppIdFromFolderName(zoneId, folderName) != AppConstants.AppIdNotFound)
            {
                l.A("skip, app is already installed");
                continue;
            }

            var appXml = Path.Combine(directoryPath, FolderConstants.DataFolderProtected, FolderConstants.AppDataFile);
            if (!File.Exists(appXml))
            {
                l.A("skip, App_Data/app.xml is missing");
                continue;
            }

            try
            {
                var importer = xmlImpExpFiles.New().Init(null, false);
                var importXmlReader = new ImportXmlReader(appXml, importer, l);
                var pendingAppDto = new PendingAppDto
                {
                    ServerFolder = folderName,
                    Name = importXmlReader.DisplayName,
                    Description = importXmlReader.Description,
                    Version = importXmlReader.Version,
                    Folder = importXmlReader.AppFolder
                };
                result.Add(pendingAppDto);
                l.A($"pending app {pendingAppDto.Name}, v{pendingAppDto.Version}");
            }
            catch (Exception e)
            {
                l.Ex(e);
            }
        }

        return l.ReturnAsOk(result);
    }
}
