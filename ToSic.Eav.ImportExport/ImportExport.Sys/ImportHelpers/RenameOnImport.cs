using System.Xml.Linq;
using System.Xml.XPath;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Sys.ImportHelpers;

internal class RenameOnImport: HelperBase
{
    public readonly string From;
    public readonly string To;
    internal RenameOnImport(string from, string to, ILog parentLog) : base(parentLog, "Imp.Rename")
    {
        From = from;
        To = to;
        Log.A($"user decided to install app in different folder:{to}, " +
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
        var appGuidNode = xmlDoc.XPathSelectElement("//SexyContent/Header/App")?.Attribute(AttributeNames.GuidNiceName);
        if(appGuidNode == null) throw new("app guid node not found - totally unexpected");
        var originalAppId = appGuidNode.Value;
        Log.A($"original AppID:{originalAppId}");

        // same App is already installed, so we have to change AppId 
        appGuidNode.Value = string.Empty;
        Log.A($"original AppID is now empty");

        // change folder to install app
        xmlDoc.XPathSelectElement("//SexyContent/Entities/Entity/Value[@Key='Folder']")
            .SetAttributeValue("Value", name);
        Log.A($"change folder to install app:{name}");

        // change DisplayName to install app
        xmlDoc.XPathSelectElement("//SexyContent/Entities/Entity/Value[@Key='DisplayName']")
            .SetAttributeValue("Value", name);
        Log.A($"change DisplayName to install app:{name}");

        // find Value element with OriginalId attribute
        var valueElementWithOriginalIdAttribute = appConfig.Elements(XmlConstants.ValueNode).FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr)?.Value == "OriginalId");
        // if missing add new Value element with OriginalId attribute
        if (valueElementWithOriginalIdAttribute == null)
        {
            Log.A($"Value element with OriginalId attribute is missing, so we are adding new one with OriginalId:{originalAppId}");
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
            Log.A($"current OriginalId:{originalId}");

            if (string.IsNullOrEmpty(originalId))
            {
                Log.A($"current OriginalId is empty, so adding OriginalId:{originalAppId}");
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
                Log.A($"replace first occurrence of original app name in folder relative path:{newFolderRelativePath}");
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
                Log.A($"replace first occurrence of original app name in file relative path:{newFileRelativePath}");
                fileItem.SetAttributeValue(XmlConstants.FolderNodePath, newFileRelativePath);
            }
        }

        xmlDoc.Save(xmlPath); // this is not necessary, but good to have it saved in file for debugging
    }

    internal void FixPortalFilesAdamAppFolderName(string appDirectory, bool pendingApp)
    {
        var originalAdamTempRoot = Path.Combine(appDirectory, GetFolderForSiteFiles(pendingApp), AdamConstants.AdamRootFolder, From);
        var newAdamTempRoot = Path.Combine(appDirectory, GetFolderForSiteFiles(pendingApp), AdamConstants.AdamRootFolder, To);
        if (Directory.Exists(originalAdamTempRoot))
        {
            Log.A($"rename app folder name in temp PortalFiles/adam from:{originalAdamTempRoot} to:{newAdamTempRoot}");
            Directory.Move(originalAdamTempRoot, newAdamTempRoot);
        }
    }

    internal string GetFolderForSiteFiles(bool pendingApp) => pendingApp ? Path.Combine(FolderConstants.AppDataProtectedFolder, FolderConstants.ZipFolderForSiteFiles) : FolderConstants.ZipFolderForPortalFiles;
}