using System;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Environment;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.ImportExport
{
    public sealed class XmlExporterUnknown: XmlExporter, IIsUnknown
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
}
