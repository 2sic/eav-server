﻿using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.ImportExport.Internal;

partial class XmlImportWithFiles
{
    private readonly Dictionary<int, int> _fileIdCorrectionList = new();
    private readonly Dictionary<int, int> _folderIdCorrectionList = new();


    private void PrepareFileIdCorrectionList(XElement sexyContentNode) => Log.Do(() =>
    {
        if (!sexyContentNode.Elements(XmlConstants.PortalFiles).Any())
            return;

        var portalFiles = sexyContentNode.Element(XmlConstants.PortalFiles)?.Elements(XmlConstants.FileNode);
        if (portalFiles == null) return;
        var filesAndPaths = portalFiles.ToDictionary(
            p => int.Parse(p.Attribute(XmlConstants.FileIdAttr).Value),
            v => v.Attribute(XmlConstants.FolderNodePath).Value
        );
        base.Services.Environment.MapExistingFilesToImportSet(filesAndPaths, _fileIdCorrectionList);
    });


    private void PrepareFolderIdCorrectionListAndCreateMissingFolders(XElement sexyContentNode) => Log.Do(() =>
    {
        if (!sexyContentNode.Elements(XmlConstants.FolderGroup).Any())
            return;

        var portalFiles = sexyContentNode.Element(XmlConstants.FolderGroup)?.Elements(XmlConstants.Folder);
        if (portalFiles == null) return;

        var foldersAndPath = portalFiles.ToDictionary(
            p => int.Parse(p.Attribute(XmlConstants.FolderNodeId).Value),
            v => v.Attribute(XmlConstants.FolderNodePath).Value
        );
        base.Services.Environment.CreateFoldersAndMapToImportIds(foldersAndPath, _folderIdCorrectionList, Messages);
    });


}