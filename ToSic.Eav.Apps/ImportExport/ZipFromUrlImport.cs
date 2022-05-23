using System;
using System.IO;
using System.Net;
using ToSic.Eav.Configuration;
using ToSic.Eav.Identity;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipFromUrlImport: ZipImport
    {
        #region DI Constructor

        public ZipFromUrlImport(IImportExportEnvironment environment, Lazy<XmlImportWithFiles> xmlImpExpFilesLazy, 
            IGlobalConfiguration globalConfiguration, SystemManager systemManager, IAppStates appStates)
            : base(environment, xmlImpExpFilesLazy, systemManager, appStates)
        {
            _globalConfiguration = globalConfiguration;
        }

        private readonly IGlobalConfiguration _globalConfiguration;

        #endregion

        public new ZipFromUrlImport Init(int zoneId, int? appId, bool allowCode, ILog parentLog)
        {
            base.Init(zoneId, appId, allowCode, parentLog);
            return this;
        }

        public bool ImportUrl(string packageUrl, bool isAppImport)
        {
            Log.A($"import zip from url:{packageUrl}, isApp:{isAppImport}");
            var path = _globalConfiguration.TemporaryFolder;
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
                    Log.A("Will upgrade TLS connection so we can connect with TLS 1.1 or 1.2");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;


                    Log.A($"try to download:{packageUrl} to:{destinationPath}");
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


            var temporaryDirectory = Path.Combine(_globalConfiguration.TemporaryFolder, Mapper.GuidCompress(Guid.NewGuid()).Substring(0, 8));

            using (var file = File.OpenRead(destinationPath))
                success = ImportZip(file, temporaryDirectory);

            File.Delete(destinationPath);

            return success;
        }
    }
}
