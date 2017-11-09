using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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


        protected JsonSerializer SerializerOfApp(int appId)
        {
            var app = Loader.AppPackage(appId);
            return new JsonSerializer(app, Log);
        }

        #endregion


        public IEnumerable<string> LogItems => Log.Entries.Select(e => e.Source + ">" + e.Message);
    }
}
