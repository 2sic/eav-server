﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11TestBase: HasLog
    {
        #region test preparations

        
        public EavDbContext Db;
        public Efc11Loader Loader;

        /// <inheritdoc />
        public Efc11TestBase() : base("efc11test")
        {
        }

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            Db = EavTestBase.Resolve<EavDbContext>();
            Loader = NewLoader();
            Loader.Init(Log);
        }

        public Efc11Loader NewLoader() => EavTestBase.Resolve<Efc11Loader>().UseExistingDb(Db);


        protected JsonSerializer SerializerOfApp(int appId)
        {
            var app = Loader.AppState(appId, false);
            return EavTestBase.Resolve<JsonSerializer>().Init(app, Log);
        }

        #endregion


        public IEnumerable<string> LogItems => Log.Entries.Select(e => e.Source + ">" + e.Message);
    }
}
