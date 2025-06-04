using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.ImportExport.Internal;

internal sealed class XmlExporterUnknown(XmlSerializer xmlSerializer, IAppsCatalog appsCatalog, WarnUseOfUnknown<XmlExporterUnknown> _)
    : XmlExporter(xmlSerializer, appsCatalog, LogScopes.NotImplemented, connect: []), IIsUnknown
{

    public override XmlExporter Init(AppExportSpecs specs, IAppReader appRuntime, bool appExport, string[] attrSetIds, string[] entityIds)
    {
        // do nothing
        return this;
    }

    public override void AddFilesToExportQueue()
    {
        // do nothing
    }

    protected override void AddFileAndFolderToQueue(int fileNum)
    {
        // do nothing
    }

    protected override TenantFileItem ResolveFile(int fileId)
    {
        throw new NotSupportedException("Not supported in provider 'Unknown'");
    }

    protected override string ResolveFolderId(int folderId)
    {
        throw new NotSupportedException("Not supported in provider 'Unknown'");
    }
}