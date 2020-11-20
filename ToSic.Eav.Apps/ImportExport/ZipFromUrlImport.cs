using System;
using System.IO;
using System.Net;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipFromUrlImport: ZipImport
    {
        private readonly IServerPaths _serverPaths;

        public ZipFromUrlImport(IImportExportEnvironment environment, IServerPaths serverPaths, Lazy<XmlImportWithFiles> xmlImpExpFilesLazy) : base(environment, xmlImpExpFilesLazy)
        {
            _serverPaths = serverPaths;
        }

        public new ZipFromUrlImport Init(int zoneId, int? appId, bool allowCode, ILog parentLog)
        {
            base.Init(zoneId, appId, allowCode, parentLog);
            return this;
        }

        public bool ImportUrl(string packageUrl, bool isAppImport)
        {
            Log.Add($"import zip from url:{packageUrl}, isApp:{isAppImport}");
            var path = _serverPaths.FullSystemPath(Settings.TemporaryDirectory);
            if (path == null)
                throw new NullReferenceException("path for temporary is null - this won't work");

            var tempDirectory = new DirectoryInfo(path);
            if (!tempDirectory.Exists)
                Directory.CreateDirectory(tempDirectory.FullName);

            var destinationPath = Path.Combine(tempDirectory.FullName, Path.GetRandomFileName() + ".zip");

            using (var client = new WebClient())
            {
                var initialProtocol = ServicePointManager.SecurityProtocol;
                try
                {
                    Log.Add("Will upgrade TLS connection so we can connect with TLS 1.1 or 1.2");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;


                    Log.Add($"try to download:{packageUrl} to:{destinationPath}");
                    client.DownloadFile(packageUrl, destinationPath);
                }
                catch (WebException e)
                {
                    throw new Exception("Could not download app package from '" + packageUrl + "'.", e);
                }
                finally
                {
                    ServicePointManager.SecurityProtocol = initialProtocol;
                }
                
            }
            bool success;


            var temporaryDirectory = _serverPaths.FullSystemPath(Path.Combine(Settings.TemporaryDirectory, Guid.NewGuid().ToString()));

            using (var file = File.OpenRead(destinationPath))
                success = ImportZip(file, temporaryDirectory);

            File.Delete(destinationPath);

            return success;
        }
    }
}
