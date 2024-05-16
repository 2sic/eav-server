using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{
    private readonly Dictionary<int, int> _fileIdCorrectionList = [];
    private readonly Dictionary<int, int> _folderIdCorrectionList = [];


    private bool PrepareFileIdCorrectionList(XElement sexyContentNode)
    {
        var l = Log.Fn<bool>();
        if (!sexyContentNode.Elements(XmlConstants.PortalFiles).Any())
            return l.ReturnTrue();

        var portalFiles = sexyContentNode.Element(XmlConstants.PortalFiles)?.Elements(XmlConstants.FileNode);
        if (portalFiles == null)
            return l.ReturnFalse();

        var filesAndPaths = portalFiles.ToDictionary(
            p => int.Parse(p.Attribute(XmlConstants.FileIdAttr).Value),
            v => v.Attribute(XmlConstants.FolderNodePath).Value
        );
        Services.Environment.MapExistingFilesToImportSet(filesAndPaths, _fileIdCorrectionList);
        return l.ReturnTrue();
    }


    private bool PrepareFolderIdCorrectionListAndCreateMissingFolders(XElement sexyContentNode)
    {
        var l = Log.Fn<bool>();
        if (!sexyContentNode.Elements(XmlConstants.FolderGroup).Any())
            return l.ReturnTrue();

        var portalFiles = sexyContentNode.Element(XmlConstants.FolderGroup)?.Elements(XmlConstants.Folder);
        if (portalFiles == null)
            return l.ReturnFalse();

        var foldersAndPath = portalFiles.ToDictionary(
            p => int.Parse(p.Attribute(XmlConstants.FolderNodeId).Value),
            v => v.Attribute(XmlConstants.FolderNodePath).Value
        );
        Services.Environment.CreateFoldersAndMapToImportIds(foldersAndPath, _folderIdCorrectionList, Messages);
        return l.ReturnTrue();
    }


}