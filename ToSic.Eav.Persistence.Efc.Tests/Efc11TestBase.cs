using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
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

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            Db = GetService<EavDbContext>();
            Loader = NewLoader();
        }

        public Efc11Loader NewLoader() => GetService<Efc11Loader>().UseExistingDb(Db);


        protected JsonSerializer SerializerOfApp(int appId)
        {
            var app = Loader.AppStateReaderRawTA(appId);
            return GetService<JsonSerializer>().SetApp(app);
        }

        #endregion


        public IEnumerable<string> LogItems => (Log as Log)?.Entries.Select(e => e.Source + ">" + e.Message);
    }
}
