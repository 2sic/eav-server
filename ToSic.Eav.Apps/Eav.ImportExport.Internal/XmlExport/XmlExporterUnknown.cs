using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Internal;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;

namespace ToSic.Eav.ImportExport.Internal;

internal sealed class XmlExporterUnknown: XmlExporter, IIsUnknown
{
    public XmlExporterUnknown(XmlSerializer xmlSerializer, IAppStates appStates, IContextResolver ctxResolver, WarnUseOfUnknown<XmlExporterUnknown> _) 
        : base(xmlSerializer, appStates, ctxResolver, LogScopes.NotImplemented)
    {
    }

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