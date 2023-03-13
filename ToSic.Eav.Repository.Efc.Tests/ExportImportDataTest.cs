﻿using System;
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
        private readonly AppRuntime _appRuntime;
        private readonly XmlExporter _xmlExporter;

        public ExportImportDataTest()
        {
            _appRuntime = GetService<AppRuntime>();
            _xmlExporter = GetService<XmlExporter>();
        }
        [TestMethod]
        [Ignore] // ignore for now, reason is that we don't have a mock-portal-settings provider
        public void ExportData()
        {
            var Log = new Log("TstExp");
            var zoneId = 2;
            var appId = 2;
            var appRuntime = _appRuntime.Init(appId, true);

            var fileXml = _xmlExporter.Init(zoneId, appId, appRuntime, false,
                /*contentTypeIdsString?.Split(';') ?? */Array.Empty<string>(),
                /*entityIdsString?.Split(';') ?? */Array.Empty<string>()
            ).GenerateNiceXml();

        }
    }
}
