﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Internal.XmlList;
using ToSic.Lib.Logging;
using ToSic.Eav.Repositories;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Apps.Tests.ImportExport;

[TestClass]
public class XmlTable: TestBaseDiEavFullAndDb
{

    public new static ILog Log = new Log("TstXml");
    private int AppId = 78;

    [TestMethod]
    [Ignore]
    public void XmlTable_ResolveWithFullFallback()
    {
        var exporter = BuildExporter(AppId, "BlogPost");

        // todo: need ML portal for testing

    }



    private ExportListXml BuildExporter(int appId, string ctName)
    {
        var loader = GetService<IRepositoryLoader>();
        var appPackage = loader.AppStateReaderRawTA(appId);
        var type = appPackage.ContentTypes.First(ct => ct.Name == ctName);
        return GetService<ExportListXml>().Init(appPackage, type);
    }
}