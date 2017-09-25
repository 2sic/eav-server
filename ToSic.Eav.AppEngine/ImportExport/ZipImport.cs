using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipImport
    {
        private int? _appId;
        private readonly int _zoneId;
        // ReSharper disable once NotAccessedField.Local
        private bool _allowRazor; // note: not used yet, but will be important in the future 
        private readonly IImportExportEnvironment _environment;
        public ZipImport(IImportExportEnvironment environment, int zoneId, int? appId, bool allowRazor, Log parentLog)
        {
            Log = new Log("ZipImp", parentLog, "constructor");
            _appId = appId;
            _zoneId = zoneId;
            _allowRazor = allowRazor;
            _environment = environment;

        }

        private Log Log { get; }

        //public List<ExportImportMessage> Messages => _environment.Messages;

        /// <summary>
        /// Imports a ZIP file (from stream)
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="temporaryDirectory"></param>
        /// <returns></returns>
        public bool ImportZip(Stream zipStream, string temporaryDirectory)
        {
            List<Message> messages = _environment.Messages;

            var success = true;
            Exception finalEx = null;

            try
            {
                if (!Directory.Exists(temporaryDirectory))
                    Directory.CreateDirectory(temporaryDirectory);

                // Extract ZIP archive to the temporary folder
                ExtractZipFile(zipStream, temporaryDirectory);

                var currentWorkingDir = temporaryDirectory;
                var baseDirectories = Directory.GetDirectories(currentWorkingDir);

                // Loop through each root-folder. For now only contains the "Apps" folder.
                foreach (var directoryPath in baseDirectories)
                {
                    switch (Path.GetFileName(directoryPath))
                    {
                        // Handle the App folder
                        case "Apps":
                            currentWorkingDir = Path.Combine(currentWorkingDir, "Apps");

                            // Loop through each app directory
                            foreach (var appDirectory in Directory.GetDirectories(currentWorkingDir))
                            {

                                var appId = new int?();

                                // Stores the number of the current xml file to process
                                var xmlIndex = 0;

                                // Import XML file(s)
                                foreach (var xmlFileName in Directory.GetFiles(appDirectory, "*.xml"))
                                {
                                    var fileContents = File.ReadAllText(Path.Combine(appDirectory, xmlFileName));
	                                var doc = XDocument.Parse(fileContents);
                                    var import = new XmlImportWithFiles(Log);//_environment.DefaultLanguage);

									if (!import.IsCompatible(doc))
										throw new Exception("The app / package is not compatible with this version of eav and the 2sxc-host.");

									var isAppImport = doc.Element(XmlConstants.RootNode).Element(XmlConstants.Header).Elements(XmlConstants.App).Any() 
                                        && doc.Element(XmlConstants.RootNode).Element(XmlConstants.Header).Element(XmlConstants.App).Attribute(XmlConstants.Guid).Value != XmlConstants.AppContentGuid /* "Default" */;

                                    if (!isAppImport && !_appId.HasValue)
                                        _appId = new ZoneRuntime(_zoneId, Log).DefaultAppId;

                                    if (isAppImport)
                                    {
                                        var appConfig = XDocument.Parse(fileContents).Element(XmlConstants.RootNode)
                                            .Element(XmlConstants.Entities)
                                            .Elements(XmlConstants.Entity)
                                            .Single(e => e.Attribute(XmlConstants.AttSetStatic).Value == "2SexyContent-App");

                                        #region Version Checks (new in 08.03.03)
                                        var reqVersionNode = appConfig.Elements(XmlConstants.ValueNode)?.FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr).Value == "RequiredVersion")?.Attribute(XmlConstants.ValueAttr)?.Value;
                                        var reqVersionNodeDnn = appConfig.Elements(XmlConstants.ValueNode)?.FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr).Value == "RequiredDnnVersion")?.Attribute(XmlConstants.ValueAttr)?.Value;

                                        CheckRequiredEnvironmentVersions(reqVersionNode, reqVersionNodeDnn);
                                        #endregion

                                        var folder = appConfig.Elements(XmlConstants.ValueNode).First(v => v.Attribute(XmlConstants.KeyAttr).Value == "Folder").Attribute(XmlConstants.ValueAttr).Value;

                                        // Do not import (throw error) if the app directory already exists
                                        var appPath = _environment.TargetPath(folder);
                                        if (Directory.Exists(appPath))
                                        {
                                            throw new Exception("The app could not be installed because the app-folder '" + appPath + "' already exists. Please remove or rename the folder in the [portals]/2sxc and install the app again.");
                                        }

                                        if (xmlIndex == 0)
                                        {
                                            // Handle PortalFiles folder
                                            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
                                            if (Directory.Exists(portalTempRoot))
                                                _environment.TransferFilesToTennant(portalTempRoot, "");
                                        }

                                        import.ImportApp(_zoneId, doc, out appId);
                                    }
                                    else
                                    {
                                        appId = _appId.Value;
                                        if (xmlIndex == 0 && import.IsCompatible(doc))
                                        {
                                            // Handle PortalFiles folder
                                            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
                                            if (Directory.Exists(portalTempRoot))
                                                _environment.TransferFilesToTennant(portalTempRoot, "");
                                        }

                                        import.ImportXml(_zoneId, appId.Value, doc);
                                    }

                                    
                                    messages.AddRange(import.ImportLog);

                                    xmlIndex++;
                                }

                                // Copy all files in 2sexy folder to (portal file system) 2sexy folder
                                var templateRoot = _environment.TemplatesRoot(_zoneId, appId.Value);// server.MapPath(Internal.TemplateManager.GetTemplatePathRoot(Settings.TemplateLocations.PortalFileSystem, app));
                                var appTemplateRoot = Path.Combine(appDirectory, "2sexy");
                                if (Directory.Exists(appTemplateRoot))
                                    new FileManager(appTemplateRoot).CopyAllFiles(templateRoot, false, messages);

                            }

                            // Reset CurrentWorkingDir
                            currentWorkingDir = temporaryDirectory;
                            break;
                    }
                }
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
                catch(Exception ex) when (ex is FormatException || ex is OverflowException) 
                {
                    // The folder itself or files inside may be used by other processes.
                    // Deleting the folder recursively will fail in such cases
                    // If deleting is not possible, just leave the temporary folder as it is
                }
            }

            if (finalEx != null)
                throw finalEx; // must throw, to enable logging outside

            return success;
        }

        private void CheckRequiredEnvironmentVersions(string reqVersionNode, string reqVersionNodeDnn)
        {
            if (reqVersionNode != null)
            {
                var vSxc = Version.Parse(_environment.ModuleVersion);// Settings.Version;
                var reqSxcV = Version.Parse(reqVersionNode);
                if (reqSxcV.CompareTo(vSxc) == 1) // required is bigger
                    throw new Exception("this app requires 2sxc version " + reqVersionNode +
                                        ", installed is " + vSxc + ". cannot continue. see also 2sxc.org/en/help?tag=app");
            }

            if (reqVersionNodeDnn != null)
            {
                var vDnn = _environment.TennantVersion;
                var reqDnnV = Version.Parse(reqVersionNodeDnn);
                if (reqDnnV.CompareTo(vDnn) == 1) // required is bigger
                    throw new Exception("this app requires dnn version " + reqVersionNodeDnn +
                                        ", installed is "+vDnn +". cannot continue. see also 2sxc.org/en/help?tag=app");
            }
        }

        public bool ImportZipFromUrl(string packageUrl, bool isAppImport)
        {
            var tempDirectory = new DirectoryInfo(HttpContext.Current.Server.MapPath(Settings.TemporaryDirectory));
            if (!tempDirectory.Exists)
                Directory.CreateDirectory(tempDirectory.FullName);

            var destinationPath = Path.Combine(tempDirectory.FullName, Path.GetRandomFileName() + ".zip");
            var client = new WebClient();
            var success = false;

            try
            {
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

        #region Zip Import Helpers



        /// <summary>
        /// Extracts a Zip (as Stream) to the given OutFolder directory.
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="outFolder"></param>
        private void ExtractZipFile(Stream zipStream, string outFolder)
        {
            var file = new ZipFile(zipStream);

            try
            {
                foreach (ZipEntry entry in file)
                {
                    if (entry.IsDirectory)
                        continue;
                    var fileName = entry.Name;

                    var entryStream = file.GetInputStream(entry);

                    var fullPath = Path.Combine(outFolder, fileName);
                    var directoryName = Path.GetDirectoryName(fullPath);
                    if (!String.IsNullOrEmpty(directoryName))
                        Directory.CreateDirectory(directoryName);

                    // Unzip File in buffered chunks
                    using (var streamWriter = File.Create(fullPath))
                    {
                        entryStream.CopyTo(streamWriter, 4096);
                    }
                }
            }
            finally
            {
                if (file != null)
                    file.Close();
            }
        }

        #endregion
    }
}