﻿using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Tests;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File.Tests
{
    [TestClass]
    public class TypeExporter: PersistenceTestsBase
    {

        [TestMethod]
        public void TypeExp_AllSharedFromInstallation()
        {
            var test = new TestValuesOnPc2Dm();
            //var dbc = Factory.Resolve<DbDataController>().Init(null, test.AppId, Log);


            var loader = Factory.Resolve<Efc11Loader>();//.Init(dbc.SqlDb);
            var app = loader.AppState(test.RootAppId);


            var cts = app.ContentTypes;
            var sharedCts = cts.Where(ct => (ct as ContentType).AlwaysShareConfiguration).ToList();

            var fileSysLoader = new FileSystemLoader(ExportStorageRoot, RepositoryTypes.TestingDoNotUse, true, null, Log);

            var time = Stopwatch.StartNew();
            sharedCts.ForEach(ct => fileSysLoader.SaveContentType(ct));
            time.Stop();

            Trace.WriteLine("created " + sharedCts.Count + "items and put into " + ExportStorageRoot);
            Trace.WriteLine("elapsed: " + time.Elapsed);
        }


    }
}
