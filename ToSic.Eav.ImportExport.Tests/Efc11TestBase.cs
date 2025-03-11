using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;
﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;
using ToSic.Testing.Shared;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.Efc.Tests;

[TestClass]
public class Efc11TestBase : TestBaseDiEavFullAndDb
{
    #region test preparations


    public EavDbContext Db;
    public EfcAppLoader Loader;

    [TestInitialize]
    public void Init()
    {
        Trace.Write("initializing DB & loader");
        Db = GetService<EavDbContext>();
        Loader = NewLoader();
    }

    public EfcAppLoader NewLoader() => GetService<EfcAppLoader>().UseExistingDb(Db);


    protected JsonSerializer SerializerOfApp(int appId)
    {
        var app = Loader.AppStateReaderRawTac(appId);
        return GetService<JsonSerializer>().SetApp(app);
    }

    #endregion


    public IEnumerable<string> LogItems => (Log as Log)?.Entries.Select(e => e.Source + ">" + e.Message);
}