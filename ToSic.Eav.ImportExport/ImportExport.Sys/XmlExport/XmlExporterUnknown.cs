using ToSic.Eav.Context;
using ToSic.Eav.Context.Internal;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.ImportExport.Internal;

internal sealed class XmlExporterUnknown(XmlSerializer xmlSerializer, IAppsCatalog appsCatalog, IContextResolver ctxResolver, WarnUseOfUnknown<XmlExporterUnknown> _)
    : XmlExporter(xmlSerializer, appsCatalog, ctxResolver, LogScopes.NotImplemented), IIsUnknown
{
    protected override void PostContextInit(IContextOfApp appContext)
    {
        /* do nothing */
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