using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Sys.Xml;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.ImportExport.Sys.XmlExport;

//internal sealed class XmlExporterUnknown(XmlSerializer xmlSerializer, IAppsCatalog appsCatalog, WarnUseOfUnknown<XmlExporterUnknown> _)
//    : XmlExporter(xmlSerializer, appsCatalog, LogScopes.NotImplemented, connect: []), IIsUnknown
//{

//    public override XmlExporter Init(AppExportSpecs specs, IAppReader appReader, bool appExport, string[] attrSetIds, string[] entityIds)
//    {
//        // do nothing
//        return this;
//    }

    //public override void AddFilesToExportQueue()
    //{
    //    // do nothing
    //}

    //protected override void AddFileAndFolderToQueue(int fileNum)
    //{
    //    // do nothing
    //}

    //protected override TenantFileItem ResolveFile(int fileId)
    //{
    //    throw new NotSupportedException("Not supported in provider 'Unknown'");
    //}

    //protected override string ResolveFolderId(int folderId)
    //{
    //    throw new NotSupportedException("Not supported in provider 'Unknown'");
    //}
//}

internal sealed class XmlExporterIntegrationUnknown(WarnUseOfUnknown<XmlExporterIntegrationUnknown> _)
    : IXmlExporterIntegration, IIsUnknown
{
    public List<int> ReferencedFileIds { get; } = [];
    public List<int> ReferencedFolderIds { get; } = [];

    /// <summary>
    /// Post context init the caller must be able to init Adam, which is not part of this project, so we're handling it as a callback
    /// </summary>
    /// <param name="appContext"></param>
    public void Setup(IContextOfApp appContext)
    {
        // do nothing
    }

    public void AddFilesToExportQueue()
    {
        // do nothing
    }

    public void AddFileAndFolderToQueue(int fileNum)
    {
        // do nothing
    }

    public TenantFileItem ResolveFile(int fileId) => throw new NotSupportedException("Not supported in provider 'Unknown'");

    public string ResolveFolderId(int folderId) => throw new NotSupportedException("Not supported in provider 'Unknown'");
}