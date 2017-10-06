using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.App;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11TestBase: HasLog
    {
        #region test preparations

        
        public EavDbContext Db;
        public Efc11Loader Loader;

        /// <inheritdoc />
        public Efc11TestBase() : base("efc11test") { }

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            Db = Factory.Resolve<EavDbContext>();
            Loader = NewLoader();
        }

        public Efc11Loader NewLoader() => new Efc11Loader(Db);


        public static JsonSerializer SerializerOfApp(AppDataPackage app)
        {
            var exBuilder = new JsonSerializer();
            exBuilder.Initialize(app);
            return exBuilder;
        }

        protected JsonSerializer SerializerOfApp(int appId)
        {
            var app = Loader.AppPackage(appId);
            return SerializerOfApp(app);
        }

        #endregion


        public IEnumerable<string> LogItems => Log.Entries.Select(e => e.Source + ">" + e.Message);
    }
}
