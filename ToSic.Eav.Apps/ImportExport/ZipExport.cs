using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Zip;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipExport
    {
        private readonly int _appId;
        private readonly int _zoneId;
        private const string SexyContentContentGroupName = "2SexyContent-ContentGroup";
        private const string SourceControlDataFolder = Constants.FolderData;
        private const string SourceControlDataFile = "app.xml"; // lower case
        private readonly string _blankGuid = Guid.Empty.ToString();
        private const string ZipFolderForPortalFiles = "PortalFiles";
        private const string ZipFolderForAppStuff = "2sexy";
        private const string AppXmlFileName = "App.xml";

        public FileManager FileManager;
        private readonly string _physicalAppPath;
        private readonly string _appFolder;

        protected ILog Log;

        public ZipExport(int zoneId, int appId, string appFolder, string physicalAppPath, ILog parentLog)
        {
            _appId = appId;
            _zoneId = zoneId;
            _appFolder = appFolder;
            _physicalAppPath = physicalAppPath;
            Log = new Log("Zip.Exp", parentLog);
            FileManager = new FileManager(_physicalAppPath);
        }

        public void ExportForSourceControl(bool includeContentGroups = false, bool resetAppGuid = false)
        {
            var path = _physicalAppPath + "\\" + SourceControlDataFolder;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // generate the XML & save
            var xmlExport = GenerateExportXml(includeContentGroups, resetAppGuid);
            var xml = xmlExport.GenerateNiceXml();
            File.WriteAllText(Path.Combine(path, SourceControlDataFile), xml);

        }

        public MemoryStream ExportApp(bool includeContentGroups = false, bool resetAppGuid = false)
        {
            // generate the XML
            var xmlExport = GenerateExportXml(includeContentGroups, resetAppGuid);

            #region Copy needed files to temporary directory

            var messages = new List<Message>();
            var randomShortFolderName = Guid.NewGuid().ToString().Substring(0, 4);

            var temporaryDirectoryPath = GetPathMultiTarget(Path.Combine(Settings.TemporaryDirectory, randomShortFolderName));
            //var temporaryDirectoryPath = HttpContext.Current.Server.MapPath(Path.Combine(Settings.TemporaryDirectory, randomShortFolderName));

            if (!Directory.Exists(temporaryDirectoryPath))
                Directory.CreateDirectory(temporaryDirectoryPath);

            AddInstructionsToPackageFolder(temporaryDirectoryPath);

            var tempDirectory = new DirectoryInfo(temporaryDirectoryPath);
            var appDirectory = tempDirectory.CreateSubdirectory("Apps/" + _appFolder + "/");
            
            var sexyDirectory = appDirectory.CreateSubdirectory(ZipFolderForAppStuff);
            
            var portalFilesDirectory = appDirectory.CreateSubdirectory(ZipFolderForPortalFiles);

            // Copy app folder
            if (Directory.Exists(_physicalAppPath))
            {
                FileManager.CopyAllFiles(sexyDirectory.FullName, false, messages);
            }

            // Copy PortalFiles
            foreach (var file in xmlExport.ReferencedFiles)
            {
                var portalFilePath = Path.Combine(portalFilesDirectory.FullName, Path.GetDirectoryName(file.RelativePath));

                if (!Directory.Exists(portalFilePath))
                    Directory.CreateDirectory(portalFilePath);

                if (File.Exists(file.Path))
                {
                    var fullPath = Path.Combine(portalFilesDirectory.FullName, file.RelativePath);
                    try
                    {
                        File.Copy(file.Path, fullPath);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error on " + fullPath + " (" + fullPath.Length + ")", e);
                    }
                }
            }
            #endregion


            // Save export xml
            var xml = xmlExport.GenerateNiceXml();
            File.AppendAllText(Path.Combine(appDirectory.FullName, AppXmlFileName), xml);

            // Zip directory and return as stream
            var stream = new Zipping(Log).ZipDirectoryIntoStream(tempDirectory.FullName + "\\");

            tempDirectory.Delete(true);

            return stream;
        }



        private XmlExporter GenerateExportXml(bool includeContentGroups, bool resetAppGuid)
        {
            // Get Export XML
            // 2020-01-17 2dm: added new parameter showDrafts and using true here, but not sure if this is actually good
            // will have to wait and see
            var runtime = new AppRuntime(new AppIdentity(_zoneId, _appId), true, Log);
            var attributeSets = runtime.ContentTypes.FromScope(includeAttributeTypes: true);
            attributeSets = attributeSets.Where(a => !((a as IContentTypeShared)?.AlwaysShareConfiguration ?? false));

            var contentTypeNames = attributeSets.Select(p => p.StaticName).ToArray();
            var templateTypeId = SystemRuntime.MetadataType(Settings.TemplateContentType);
            var entities =
                new DataSource(Log).GetPublishing(runtime, false).Out[Constants.DefaultStreamName].List.Where(
                    e => e.MetadataFor.TargetType != templateTypeId
                         && e.MetadataFor.TargetType != Constants.MetadataForAttribute).ToList();

            if (!includeContentGroups)
                entities = entities.Where(p => p.Type.StaticName != SexyContentContentGroupName).ToList();

            var entityIds = entities
                .Select(e => e.EntityId.ToString()).ToArray();


            var xmlExport = Factory.Resolve<XmlExporter>()
                .Init(_zoneId, _appId, runtime, true, contentTypeNames, entityIds, Log);

            #region reset App Guid if necessary

            if (resetAppGuid)
            {
                var root = xmlExport.ExportXDocument; //.Root;
                var appGuid = root.XPathSelectElement("/SexyContent/Header/App").Attribute("Guid");
                appGuid.Value = _blankGuid;
            }
            return xmlExport;
            #endregion
        }

        /// <summary>
        /// This adds various files to an app-package, so anybody who gets such a package
        /// is informed as to what they must do with it.
        /// </summary>
        /// <param name="targetPath"></param>
        private void AddInstructionsToPackageFolder(string targetPath)
        {
            var srcPath = GetPathMultiTarget(Path.Combine(Settings.ModuleDirectory, "SexyContent\\ImportExport\\Instructions"));
            //var srcPath = HttpContext.Current.Server.MapPath(Path.Combine(Settings.ModuleDirectory, "SexyContent\\ImportExport\\Instructions"));

            foreach (var file in Directory.GetFiles(srcPath))
                File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)));
        }

        internal static string GetPathMultiTarget(string path)
        {
#if NET451
            return HttpContext.Current.Server.MapPath(path);
#else
            return "Not Yet Implemented in .net standard #TodoNetStandard";
#endif
        }

    }
}