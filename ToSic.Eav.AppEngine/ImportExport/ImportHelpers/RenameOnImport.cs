using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.ImportExport.ImportHelpers
{
    internal class RenameOnImport: HasLog
    {
        public readonly string From;
        public readonly string To;
        internal RenameOnImport(string from, string to, ILog parentLog) : base("Imp.Rename", parentLog)
        {
            From = from;
            To = to;
            Log.Add($"user decided to install app in different folder:{to}, " +
                $"because app is already installed in folder:{from}");

        }

        internal void FixAppXmlForImportAsDifferentApp(ImportXmlReader imp)
        {
            var name = To;
            var xmlDoc = imp.XmlDoc;
            var appConfig = imp.AppConfig;
            var xmlPath = imp.XmlPath;
            // save original App folder
            var originalFolder = appConfig.Elements(XmlConstants.ValueNode).First(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "Folder").Attribute(XmlConstants.ValueAttr)?.Value;

            // save original AppId (because soon will be rewritten with empty string)
            var appGuidNode = xmlDoc.XPathSelectElement("//SexyContent/Header/App")?.Attribute("Guid");
            if(appGuidNode == null) throw new Exception("app guid node not found - totally unexpected");
            var originalAppId = appGuidNode.Value;
            Log.Add($"original AppID:{originalAppId}");

            // same App is already installed, so we have to change AppId 
            appGuidNode.Value = string.Empty;//.SetAttributeValue("Guid", string.Empty);
            Log.Add($"original AppID is now empty");

            // change folder to install app
            xmlDoc.XPathSelectElement("//SexyContent/Entities/Entity/Value[@Key='Folder']")
                .SetAttributeValue("Value", name);
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
            var folders = xmlDoc.Element(XmlConstants.RootNode)?.Elements(XmlConstants.FolderGroup)?.FirstOrDefault();
            if (folders != null)
            {
                foreach (var folderItem in folders?.Elements(XmlConstants.Folder)?.ToList())
                {
                    var originalFolderRelativePath = folderItem.Attribute(XmlConstants.FolderNodePath).Value;
                    // replace first occurrence of original app name in folder relative path with new name
                    var position = originalFolderRelativePath.IndexOf(originalFolder);
                    if (position == -1) continue;
                    var newFolderRelativePath = originalFolderRelativePath.Remove(position, originalFolder.Length).Insert(position, name);
                    Log.Add($"replace first occurrence of original app name in folder relative path:{newFolderRelativePath}");
                    folderItem.SetAttributeValue(XmlConstants.FolderNodePath, newFolderRelativePath);
                }
            }

            // change app folder in PortalFiles
            var files = xmlDoc.Element(XmlConstants.RootNode)?.Elements(XmlConstants.PortalFiles)?.FirstOrDefault();
            if (files != null)
            {
                foreach (var fileItem in files?.Elements(XmlConstants.FileNode)?.ToList())
                {
                    var originalFileRelativePath = fileItem.Attribute(XmlConstants.FolderNodePath).Value;
                    // replace first occurrence of original app name in file relative path with new name
                    var position = originalFileRelativePath.IndexOf(originalFolder);
                    if (position == -1) continue;
                    var newFileRelativePath = originalFileRelativePath.Remove(position, originalFolder.Length).Insert(position, name);
                    Log.Add($"replace first occurrence of original app name in file relative path:{newFileRelativePath}");
                    fileItem.SetAttributeValue(XmlConstants.FolderNodePath, newFileRelativePath);
                }
            }

            xmlDoc.Save(xmlPath); // this is not necessary, but good to have it saved in file for debugging
        }

        internal void FixPortalFilesAdamAppFolderName(string appDirectory)
        {
            var originalAdamTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles, "adam", From);
            var newAdamTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles, "adam", To);
            if (Directory.Exists(originalAdamTempRoot))
            {
                Log.Add($"rename app folder name in temp PortalFiles/adam from:{originalAdamTempRoot} to:{newAdamTempRoot}");
                Directory.Move(originalAdamTempRoot, newAdamTempRoot);
            }
        }
    }
}
