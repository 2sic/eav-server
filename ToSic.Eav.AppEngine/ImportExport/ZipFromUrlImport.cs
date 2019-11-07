using System;
using System.IO;
using System.Net;
using System.Web;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipFromUrlImport: ZipImport
    {
        public ZipFromUrlImport(IImportExportEnvironment environment, int zoneId, int? appId, bool allowCode, ILog parentLog) 
            : base(environment, zoneId, appId, allowCode, parentLog)
        {
        }

        public bool ImportUrl(string packageUrl, bool isAppImport)
        {
            Log.Add($"import zip from url:{packageUrl}, isApp:{isAppImport}");
            var path = HttpContext.Current.Server.MapPath(Settings.TemporaryDirectory);
            if (path == null)
                throw new NullReferenceException("path for temporary is null - this won't work");

            var tempDirectory = new DirectoryInfo(path);
            if (!tempDirectory.Exists)
                Directory.CreateDirectory(tempDirectory.FullName);

            var destinationPath = Path.Combine(tempDirectory.FullName, Path.GetRandomFileName() + ".zip");

            using (var client = new WebClient())
            {
                try
                {
                    Log.Add($"try to download:{packageUrl} to:{destinationPath}");
                    client.DownloadFile(packageUrl, destinationPath);
                }
                catch (WebException e)
                {
                    throw new Exception("Could not download app package from '" + packageUrl + "'.", e);
                }
                
            }
            bool success;


            var temporaryDirectory = HttpContext.Current.Server.MapPath(Path.Combine(Settings.TemporaryDirectory, Guid.NewGuid().ToString()));
            // Increase script timeout to prevent timeouts
            HttpContext.Current.Server.ScriptTimeout = 300;

            using (var file = File.OpenRead(destinationPath))
                success = ImportZip(file, temporaryDirectory);

            File.Delete(destinationPath);

            return success;
        }
    }
}
