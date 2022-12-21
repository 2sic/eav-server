﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataSources;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Zip;
using ToSic.Lib.Logging;

using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipExport: ServiceBase
    {
        private int _appId;
        private int _zoneId;
        private const string SexyContentContentGroupName = "2SexyContent-ContentGroup";
        private const string SourceControlDataFolder = Constants.AppDataProtectedFolder;
        private const string SourceControlDataFile = Constants.AppDataFile;
        private readonly string _blankGuid = Guid.Empty.ToString();

        public FileManager FileManager;
        private string _physicalAppPath;
        private string _appFolder;

        public FileManager FileManagerGlobal;
        private string _physicalPathGlobal;

        protected ILog Log;

        #region DI Constructor

        public ZipExport(AppRuntime appRuntime,
            DataSourceFactory dataSourceFactory,
            XmlExporter xmlExporter,
            IGlobalConfiguration globalConfiguration): base(LogNames.Eav + ".ZipExp")
        {
            ConnectServices(
                _xmlExporter = xmlExporter,
                _globalConfiguration = globalConfiguration,
                AppRuntime = appRuntime,
                DataSourceFactory = dataSourceFactory
            );
        }

        private readonly XmlExporter _xmlExporter;
        private readonly IGlobalConfiguration _globalConfiguration;
        private AppRuntime AppRuntime { get; }
        public DataSourceFactory DataSourceFactory { get; }

        public ZipExport Init(int zoneId, int appId, string appFolder, string physicalAppPath, string physicalPathGlobal, ILog parentLog)
        {
            _appId = appId;
            _zoneId = zoneId;
            _appFolder = appFolder;
            _physicalAppPath = physicalAppPath;
            _physicalPathGlobal = physicalPathGlobal;
            Log = new Log("Zip.Exp", parentLog);
            FileManager = new FileManager(_physicalAppPath).Init(Log);
            FileManagerGlobal = new FileManager(physicalPathGlobal).Init(Log);
            AppRuntime.InitQ(new AppIdentity(_zoneId, _appId), true);
            return this;
        }
        #endregion

        public void ExportForSourceControl(bool includeContentGroups = false, bool resetAppGuid = false, bool withSiteFiles = false)
        {
            var appDataPath = Path.Combine(_physicalAppPath, SourceControlDataFolder);

            // migrate old .data to App_Data also here
            // to ensure that older export is overwritten
            ZipImport.MigrateOldAppDataFile(_physicalAppPath);

            // create App_Data unless exists
            Directory.CreateDirectory(appDataPath);

            // generate the XML & save
            var xmlExport = GenerateExportXml(includeContentGroups, resetAppGuid);

            if (withSiteFiles)
            {
                var appDataDirectory = new DirectoryInfo(appDataPath);

                // 1. Copy app global templates folder for version control
                if (Directory.Exists(_physicalPathGlobal))
                {
                    // Sometimes delete is locked by external process
                    try
                    {
                        // Empty older version of app global templates state in App_Data
                        var globalTemplatesStatePath = Path.Combine(appDataPath, Constants.ZipFolderForGlobalAppStuff);
                        ZipImport.TryToDeleteDirectory(globalTemplatesStatePath, Log);
                        // Version control folder to preserve copy of app global templates
                        var globalTemplatesStateFolder = appDataDirectory.CreateSubdirectory(Constants.ZipFolderForGlobalAppStuff);

                        // Copy app global templates for version control
                        var _ = new List<Message>();
                        FileManagerGlobal.CopyAllFiles(globalTemplatesStateFolder.FullName, true, _);
                    }
                    catch (Exception e)
                    {
                        Log.Ex(e);
                    }
                }

                // 2. Copy SiteFiles for version control
                try
                {
                    // Empty older version of SiteFiles state in App_Data
                    var portalFilesPath = Path.Combine(appDataPath, Constants.ZipFolderForSiteFiles);
                    ZipImport.TryToDeleteDirectory(portalFilesPath, Log);

                    // Version control folder to preserve copy of SiteFiles
                    var portalFilesDirectory = appDataDirectory.CreateSubdirectory(Constants.ZipFolderForSiteFiles);

                    // Copy SiteFiles for version control
                    CopyPortalFiles(xmlExport, portalFilesDirectory);
                }
                catch (Exception e)
                {
                    Log.Ex(e);
                }
            }

            var xml = xmlExport.GenerateNiceXml();
            File.WriteAllText(Path.Combine(appDataPath, SourceControlDataFile), xml);
        }

        public MemoryStream ExportApp(bool includeContentGroups = false, bool resetAppGuid = false)
        {
            // generate the XML
            var xmlExport = GenerateExportXml(includeContentGroups, resetAppGuid);

            // migrate old .data to App_Data also here
            // to ensure that older export is overwritten
            ZipImport.MigrateOldAppDataFile(_physicalAppPath);

            #region Copy needed files to temporary directory

            var messages = new List<Message>();
            var randomShortFolderName = Guid.NewGuid().ToString().Substring(0, 4);

            var temporaryDirectoryPath = Path.Combine(_globalConfiguration.TemporaryFolder, randomShortFolderName);

            Directory.CreateDirectory(temporaryDirectoryPath); // create temp dir unless exists

            AddInstructionsToPackageFolder(temporaryDirectoryPath);

            var tempDirectory = new DirectoryInfo(temporaryDirectoryPath);
            var appDirectory = tempDirectory.CreateSubdirectory("Apps/" + _appFolder + "/");

            var sexyDirectory = appDirectory.CreateSubdirectory(Constants.ZipFolderForAppStuff);
            var globalSexyDirectory = appDirectory.CreateSubdirectory(Constants.ZipFolderForGlobalAppStuff);
            var siteFilesDirectory = appDirectory.CreateSubdirectory(Constants.ZipFolderForPortalFiles);



            // Copy app folder
            if (Directory.Exists(_physicalAppPath))
                FileManager.CopyAllFiles(sexyDirectory.FullName, false, messages);

            // Copy global app folder only for ParentApp
            var parentAppGuid = xmlExport.AppState.ParentApp.AppState?.NameId;
            if (parentAppGuid == null || AppStateExtensions.AppGuidIsAPreset(parentAppGuid))
                if (Directory.Exists(_physicalPathGlobal))
                    FileManagerGlobal.CopyAllFiles(globalSexyDirectory.FullName, false, messages);

            // Copy SiteFiles
            CopyPortalFiles(xmlExport, siteFilesDirectory);
            #endregion

            // create tmp App_Data unless exists
            var tmpAppDataProtectedFolder = Path.Combine(appDirectory.FullName, Constants.ToSxcFolder, Constants.AppDataProtectedFolder);
            Directory.CreateDirectory(tmpAppDataProtectedFolder);

            // Save export xml
            var xml = xmlExport.GenerateNiceXml();
            File.WriteAllText(Path.Combine(tmpAppDataProtectedFolder, Constants.AppDataFile), xml);

            // Zip directory and return as stream
            var stream = new Zipping(Log).ZipDirectoryIntoStream(tempDirectory.FullName + "\\");

            ZipImport.TryToDeleteDirectory(temporaryDirectoryPath, Log);

            return stream;
        }

        private static void CopyPortalFiles(XmlExporter xmlExport, DirectoryInfo siteFilesDirectory)
        {
            foreach (var file in xmlExport.ReferencedFiles)
            {
                var portalFilePath = Path.Combine(siteFilesDirectory.FullName, Path.GetDirectoryName(file.RelativePath));

                Directory.CreateDirectory(portalFilePath);

                if (!File.Exists(file.Path)) continue;

                var fullPath = Path.Combine(siteFilesDirectory.FullName, file.RelativePath);
                try
                {
                    File.Copy(file.Path, fullPath, overwrite: true);
                }
                catch (Exception e)
                {
                    throw new Exception("Error on " + fullPath + " (" + fullPath.Length + ")", e);
                }
            }
        }


        private XmlExporter GenerateExportXml(bool includeContentGroups, bool resetAppGuid)
        {
            // Get Export XML
            var runtime = AppRuntime.Init(Log).InitQ(new AppIdentity(_zoneId, _appId), true);
            var attributeSets = runtime.ContentTypes.All.OfScope(includeAttributeTypes: true);
            attributeSets = attributeSets.Where(a => !((a as IContentTypeShared)?.AlwaysShareConfiguration ?? false));

            // Exclude ParentApp attributeSets
            // TODO: option to include ParentApp attributeSets
            attributeSets = attributeSets.Where(p => !p.HasAncestor());

            var contentTypeNames = attributeSets.Select(p => p.NameId).ToArray();

            // 2022-01-04 2dm Cleaned up
            // This was for a very old way of storing Template information, probably 2sxc 1-4 or something
            // I'll completely disable this, as I believe it's not in use at all 
            // Keep this commented till End of June 2022 #cleanUp #oldTemplates #2631
            //var templateTypeId = 15; // _metaTargetTypes.GetId(Settings.TemplateContentType);
            //var entities =
            //    DataSourceFactory.GetPublishing(runtime, false).List.Where(
            //        e => e.MetadataFor.TargetType != templateTypeId
            //             && e.MetadataFor.TargetType != (int)TargetTypes.Attribute).ToList();

            // 2022-01-04 2dm - new code, simplified
            // Get all entities except Attribute/Field Metadata, which is exported in a different way
            var entities = DataSourceFactory.GetPublishing(runtime, false).List
                    .Where(e => e.MetadataFor.TargetType != (int)TargetTypes.Attribute).ToList();

            if (!includeContentGroups)
                entities = entities.Where(p => p.Type.NameId != SexyContentContentGroupName).ToList();

            // Exclude ParentApp entities
            // TODO: option to include ParentApp entities
            entities = entities.Where(p => !p.HasAncestor()).ToList();

            var entityIds = entities
                .Select(e => e.EntityId.ToString()).ToArray();

            var xmlExport = _xmlExporter.Init(_zoneId, _appId, runtime, true, contentTypeNames, entityIds, Log);

            #region reset App Guid if necessary

            if (resetAppGuid)
            {
                var root = xmlExport.ExportXDocument; //.Root;
                var appGuid = root.XPathSelectElement("/SexyContent/Header/App").Attribute(XmlConstants.Guid);
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
            var srcPath = _globalConfiguration.InstructionsFolder;

            foreach (var file in Directory.GetFiles(srcPath))
                File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)));
        }
    }
}