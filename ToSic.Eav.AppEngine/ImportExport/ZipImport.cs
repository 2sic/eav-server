using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Zip;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipImport
    {
        private int? _appId;
        private readonly int _zoneId;
        private readonly IImportExportEnvironment _environment;
        public ZipImport(IImportExportEnvironment environment, int zoneId, int? appId, bool allowRazor, Log parentLog)
        {
            Log = new Log("Zip.Imp", parentLog);
            _appId = appId;
            _zoneId = zoneId;
            _environment = environment;

        }

        private Log Log { get; }

        //public List<ExportImportMessage> Messages => _environment.Messages;

        /// <summary>
        /// Imports a ZIP file (from stream)
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="temporaryDirectory"></param>
        /// <param name="name">App name</param>
        /// <returns></returns>
        public bool ImportZip(Stream zipStream, string temporaryDirectory, string name = null)
        {
            Log.Add($"import zip temp-dir:{temporaryDirectory}");
            List<Message> messages = _environment.Messages;

            var success = true;
            Exception finalEx = null;

            try
            {
                if (!Directory.Exists(temporaryDirectory))
                    Directory.CreateDirectory(temporaryDirectory);

                // Extract ZIP archive to the temporary folder
                new Zipping(Log).ExtractZipFile(zipStream, temporaryDirectory);

                var currentWorkingDir = temporaryDirectory;
                var baseDirectories = Directory.GetDirectories(currentWorkingDir);

                // Loop through each root-folder. For now only contains the "Apps" folder.
                foreach (var directoryPath in baseDirectories)
                {
                    Log.Add($"folder:{directoryPath}");
                    switch (Path.GetFileName(directoryPath))
                    {
                        // Handle the App folder
                        case "Apps":
                            currentWorkingDir = Path.Combine(currentWorkingDir, "Apps");

                            // Loop through each app directory
                            foreach (var appDirectory in Directory.GetDirectories(currentWorkingDir))
                            {
                                Log.Add($"folder:{appDirectory}");

                                // Stores the number of the current xml file to process
                                var xmlIndex = 0;

                                // Import XML file(s)
                                foreach (var xmlFileName in Directory.GetFiles(appDirectory, "*.xml"))
                                {
                                    Log.Add($"xml file:{xmlFileName}");
                                    //var appId = new int?();
                                    int appId;
                                    var xmlPath = Path.Combine(appDirectory, xmlFileName);
                                    var fileContents = File.ReadAllText(xmlPath);
	                                var xdoc = XDocument.Parse(fileContents);
                                    var import = new XmlImportWithFiles(Log);

									if (!import.IsCompatible(xdoc))
										throw new Exception("The app / package is not compatible with this version of eav and the 2sxc-host.");

                                    var rootNode = xdoc.Element(XmlConstants.RootNode);
                                    if(rootNode == null) throw new NullReferenceException("xml root node couldn't be found");
                                    var headNode = rootNode.Element(XmlConstants.Header);
                                    if(headNode == null) throw new NullReferenceException("xml header node couldn't be found");

                                    var isAppImport = headNode.Elements(XmlConstants.App).Any() 
                                        && headNode.Element(XmlConstants.App)?.Attribute(XmlConstants.Guid)?.Value != XmlConstants.AppContentGuid;

                                    if (!isAppImport && !_appId.HasValue)
                                        _appId = new ZoneRuntime(_zoneId, Log).DefaultAppId;

                                    if (isAppImport)
                                    {
                                        Log.Add("will do app-import");
                                        var appConfig = rootNode
                                            .Element(XmlConstants.Entities)?
                                            .Elements(XmlConstants.Entity)
                                            .Single(e => e.Attribute(XmlConstants.AttSetStatic)?.Value == AppConstants.TypeAppConfig);

                                        if(appConfig == null)
                                            throw new NullReferenceException("app config node not found in xml, cannot continue");

                                        #region Version Checks (new in 08.03.03)
                                        var reqVersionNode = appConfig.Elements(XmlConstants.ValueNode).FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "RequiredVersion")?.Attribute(XmlConstants.ValueAttr)?.Value;
                                        var reqVersionNodeDnn = appConfig.Elements(XmlConstants.ValueNode).FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "RequiredDnnVersion")?.Attribute(XmlConstants.ValueAttr)?.Value;

                                        CheckRequiredEnvironmentVersions(reqVersionNode, reqVersionNodeDnn);
                                        #endregion

                                        var folder = appConfig.Elements(XmlConstants.ValueNode).First(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "Folder").Attribute(XmlConstants.ValueAttr)?.Value;

                                        if(folder == null)
                                            throw new NullReferenceException("can't determine folder from xml, cannot continue");

                                        // user decided to install app in different folder, because same App is already installed
                                        if (!String.IsNullOrEmpty(name))
                                        {
                                            Log.Add($"user decided to install app in different folder:{name}, because app is already installed in folder:{folder}");
                                            FixAppXmlForInportAsDifferentApp(name, xdoc, appConfig, xmlPath);
                                            FixPortalFilesAdamAppFolderName(appDirectory, folder, name);
                                            folder = name;
                                        }

                                        // Do not import (throw error) if the app directory already exists
                                        var appPath = _environment.TargetPath(folder);
                                        if (Directory.Exists(appPath))
                                            throw new IOException(
                                                "The app could not be installed because the app-folder '" + appPath +
                                                "' already exists.");

                                        if (xmlIndex == 0)
                                        {
                                            // Handle PortalFiles folder
                                            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
                                            if (Directory.Exists(portalTempRoot))
                                            {
                                                _environment.TransferFilesToTenant(portalTempRoot, string.Empty);
                                            }
                                        }

                                        import.ImportApp(_zoneId, xdoc, out appId);
                                    }
                                    else
                                    {
                                        Log.Add("will do content import");
                                        appId = _appId.Value;
                                        if (xmlIndex == 0 && import.IsCompatible(xdoc))
                                        {
                                            // Handle PortalFiles folder
                                            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
                                            if (Directory.Exists(portalTempRoot))
                                                _environment.TransferFilesToTenant(portalTempRoot, "");
                                        }

                                        import.ImportXml(_zoneId, appId, xdoc);
                                    }

                                    
                                    messages.AddRange(import.Messages);

                                    xmlIndex++;


                                    // Copy all files in 2sexy folder to (portal file system) 2sexy folder
                                    var templateRoot = _environment.TemplatesRoot(_zoneId, appId);
                                    var appTemplateRoot = Path.Combine(appDirectory, "2sexy");
                                    if (Directory.Exists(appTemplateRoot))
                                        new FileManager(appTemplateRoot).CopyAllFiles(templateRoot, false, messages);
                                }

                            }

                            // Reset CurrentWorkingDir
                            currentWorkingDir = temporaryDirectory;
                            break;
                    }
                }
            }
            catch (IOException e)
            {
                // The app could not be installed because the app-folder already exists. Install app in different folder?
                finalEx = e; // keep to throw later
                // Add error message and return false, but use MessageTypes.Warning so we can prompt user for new different name
                messages.Add(new Message("Could not import the app / package: " + e.Message, Message.MessageTypes.Warning));
                // Exceptions.LogException(e);
                success = false;
            }
            catch (Exception e)
            {
                finalEx = e; // keep to throw later
                // Add error message and return false
                messages.Add(new Message("Could not import the app / package: " + e.Message, Message.MessageTypes.Error));
                // Exceptions.LogException(e);
                success = false;
            }
            finally
            {
                try
                {
                    // Finally delete the temporary directory
                    Directory.Delete(temporaryDirectory, true);
                }
                catch
                {
                    Log.Add("Delete ran into issues, will ignore. " +
                            "Probably files/folders are used by another process like anti-virus. " +
                            "Just leave temp folder as is");
                }
            }

            if (finalEx != null)
            {
                Log.Add("had found errors during import, will throw");
                throw finalEx; // must throw, to enable logging outside
            }

            Log.Add("import zip - completed");
            return success;
        }

        private void FixAppXmlForInportAsDifferentApp(string name, XDocument xdoc, XElement appConfig, string xmlPath)
        {
            // save original App folder
            var originalFolder = appConfig.Elements(XmlConstants.ValueNode).First(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "Folder").Attribute(XmlConstants.ValueAttr)?.Value;

            // save original AppId (because soon will be rewritten with empty string)
            var originalAppId = xdoc.XPathSelectElement("//SexyContent/Header/App").Attribute("Guid").Value;
            Log.Add($"original AppID:{originalAppId}");

            // same App is already installed, so we have to change AppId 
            xdoc.XPathSelectElement("//SexyContent/Header/App").SetAttributeValue("Guid", string.Empty);
            Log.Add($"original AppID is empty");

            // change folder to install app
            xdoc.XPathSelectElement("//SexyContent/Entities/Entity/Value[@Key='Folder']").SetAttributeValue("Value", name);
            Log.Add($"change folder to install app:{name}");

            // find Value element with OriginalId attribute
            var valueElementWithOriginalIdAttribute = appConfig.Elements(XmlConstants.ValueNode).FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "OriginalId");
            // if missing add new Value element with OriginalId attribute
            if (valueElementWithOriginalIdAttribute == null)
            {
                Log.Add($"Value element with OriginalId attribute is missing, so we are adding new one with OriginalId:{originalAppId}");
                var valueElement = new XElement("Value");
                valueElement.SetAttributeValue("Key", "OriginalId");
                valueElement.SetAttributeValue("Value", originalAppId);
                valueElement.SetAttributeValue("Type", "String");
                appConfig.Add(valueElement);
            }
            else
            {
                // if OriginalID is empty, than add original AppId to it
                var originalId = valueElementWithOriginalIdAttribute.Attribute(XmlConstants.ValueAttr)?.Value;
                Log.Add($"current OriginalId:{originalId}");

                if (string.IsNullOrEmpty(originalId))
                {
                    Log.Add($"current OriginalId is empty, so adding OriginalId:{originalAppId}");
                    appConfig.Elements(XmlConstants.ValueNode).First(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "OriginalId").SetAttributeValue("OriginalId", originalAppId);
                }
            }

            // change folder in PortalFolders
            var folders = xdoc.Element(XmlConstants.RootNode)?.Elements(XmlConstants.FolderGroup)?.FirstOrDefault();
            if (folders != null)
            {
                foreach (var folderItem in folders?.Elements(XmlConstants.Folder)?.ToList())
                {
                    string originalFolderRelativePath = folderItem.Attribute(XmlConstants.FolderNodePath).Value;
                    // replace first occurrence of original app name in folder relative path with new name
                    int position = originalFolderRelativePath.IndexOf(originalFolder);
                    if (position == -1) continue;
                    string newFolderRelativePath = originalFolderRelativePath.Remove(position, originalFolder.Length).Insert(position, name);
                    Log.Add($"replace first occurrence of original app name in folder relative path:{newFolderRelativePath}");
                    folderItem.SetAttributeValue(XmlConstants.FolderNodePath, newFolderRelativePath);
                }
            }

            // change app folder in PortalFiles
            var files = xdoc.Element(XmlConstants.RootNode)?.Elements(XmlConstants.PortalFiles)?.FirstOrDefault();
            if (files != null)
            {
                foreach (var fileItem in files?.Elements(XmlConstants.FileNode)?.ToList())
                {
                    string originalFileRelativePath = fileItem.Attribute(XmlConstants.FolderNodePath).Value;
                    // replace first occurrence of original app name in file relative path with new name
                    int position = originalFileRelativePath.IndexOf(originalFolder);
                    if (position == -1) continue;
                    string newFileRelativePath = originalFileRelativePath.Remove(position, originalFolder.Length).Insert(position, name);
                    Log.Add($"replace first occurrence of original app name in file relative path:{newFileRelativePath}");
                    fileItem.SetAttributeValue(XmlConstants.FolderNodePath, newFileRelativePath);
                }
            }

            xdoc.Save(xmlPath); // this is not necessary, but good to have it saved in file for debugging
        }

        private void FixPortalFilesAdamAppFolderName(string appDirectory, string originalFolderName, string newFolderName)
        {
            var originalPortalFilesAdamAppTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles, "adam", originalFolderName);
            var newPortalFilesAdamAppTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles, "adam", newFolderName);
            if (Directory.Exists(originalPortalFilesAdamAppTempRoot))
            {
                Log.Add($"rename app folder name in temp PortalFiles/adam from:{originalPortalFilesAdamAppTempRoot} to:{newPortalFilesAdamAppTempRoot}");
                Directory.Move(originalPortalFilesAdamAppTempRoot, newPortalFilesAdamAppTempRoot);
            }
        }

        private void CheckRequiredEnvironmentVersions(string reqVersionNode, string reqVersionNodeDnn)
        {
            Log.Add($"check version requirements eav:{reqVersionNode}, host:{reqVersionNodeDnn}");
            if (reqVersionNode != null)
            {
                var vEav = Version.Parse(_environment.ModuleVersion);
                var reqEav = Version.Parse(reqVersionNode);
                if (reqEav.CompareTo(vEav) == 1) // required is bigger
                    throw new Exception("this app requires eav/2sxc version " + reqVersionNode +
                                        ", installed is " + vEav + ". cannot continue. see also 2sxc.org/en/help?tag=app");
            }

            if (reqVersionNodeDnn != null)
            {
                var vHost = _environment.TenantVersion;
                var reqHost = Version.Parse(reqVersionNodeDnn);
                if (reqHost.CompareTo(vHost) == 1) // required is bigger
                    throw new Exception("this app requires host/dnn version " + reqVersionNodeDnn +
                                        ", installed is "+vHost +". cannot continue. see also 2sxc.org/en/help?tag=app");
            }
            Log.Add("version check completed");
        }

        public bool ImportZipFromUrl(string packageUrl, bool isAppImport)
        {
            Log.Add($"import zip from url:{packageUrl}, isApp:{isAppImport}");
            var path = HttpContext.Current.Server.MapPath(Settings.TemporaryDirectory);
            if(path == null)
                throw new NullReferenceException("path for temporary is null - this won't work");

            var tempDirectory = new DirectoryInfo(path);
            if (!tempDirectory.Exists)
                Directory.CreateDirectory(tempDirectory.FullName);

            var destinationPath = Path.Combine(tempDirectory.FullName, Path.GetRandomFileName() + ".zip");

            var client = new WebClient();
            bool success;

            try
            {
                Log.Add($"try to download:{packageUrl} to:{destinationPath}");
                client.DownloadFile(packageUrl, destinationPath);
            }
            catch(WebException e)
            {
                throw new Exception("Could not download app package from '" + packageUrl + "'.", e);
            }

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