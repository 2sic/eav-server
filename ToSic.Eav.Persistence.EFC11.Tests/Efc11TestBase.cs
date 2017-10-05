using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.App;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11TestBase
    {
        #region test preparations

        public EavDbContext Db;
        public Efc11Loader Loader;

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
        #endregion

    }
}
