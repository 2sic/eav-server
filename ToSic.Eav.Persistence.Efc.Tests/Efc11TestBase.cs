using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Testing.Shared;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11TestBase: TestBaseDiEavFullAndDb
    {
        #region test preparations

        
        public EavDbContext Db;
        public Efc11Loader Loader;

        ///// <inheritdoc />
        //public Efc11TestBase() : base("efc11test")
        //{
        //}

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            Db = Build<EavDbContext>();
            Loader = NewLoader();
            Loader.Init(Log);
        }

        public Efc11Loader NewLoader() => Build<Efc11Loader>().UseExistingDb(Db);


        protected JsonSerializer SerializerOfApp(int appId)
        {
            var app = Loader.AppState(appId, false);
            return Build<JsonSerializer>().Init(app, Log);
        }

        #endregion


        public IEnumerable<string> LogItems => (Log as Log)?.Entries.Select(e => e.Source + ">" + e.Message);
    }
}
