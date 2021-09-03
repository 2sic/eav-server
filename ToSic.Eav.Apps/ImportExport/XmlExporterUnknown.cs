using System;
using ToSic.Eav.ImportExport.Environment;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.ImportExport
{
    public sealed class XmlExporterUnknown: XmlExporter, IIsUnknown
    {
        public XmlExporterUnknown(XmlSerializer xmlSerializer, IAppStates appStates) : base(xmlSerializer, appStates, LogNames.NotImplemented)
        {
        }

        public override XmlExporter Init(int zoneId, int appId, AppRuntime appRuntime, bool appExport, string[] attrSetIds, string[] entityIds,
            ILog parentLog)
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
}
