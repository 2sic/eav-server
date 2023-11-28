using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Lib.Logging;

using ToSic.Testing.Shared;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class ExportImportDataTest: TestBaseDiEavFullAndDb
    {
        private readonly XmlExporter _xmlExporter;

        public ExportImportDataTest()
        {
            _xmlExporter = GetService<XmlExporter>();
        }
        [TestMethod]
        [Ignore] // ignore for now, reason is that we don't have a mock-portal-settings provider
        public void ExportData()
        {
            var Log = new Log("TstExp");
            var zoneId = 2;
            var appId = 2;
            var appState = GetService<IAppStates>().GetReaderInternalOrNull(new AppIdentity(zoneId, appId));

            var fileXml = _xmlExporter.Init(zoneId, appId, appState, false,
                /*contentTypeIdsString?.Split(';') ?? */Array.Empty<string>(),
                /*entityIdsString?.Split(';') ?? */Array.Empty<string>()
            ).GenerateNiceXml();

        }
    }
}
