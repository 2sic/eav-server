using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class SaveDataToDbTests
    {

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
        }

        [TestMethod]
        public void LoadOneAndSaveUnchanged()
        {
            var test = new TestValuesOnPc2Dm();
            var so = SaveOptions.Build(test.ZoneId);
            var dbi = DbDataController.Instance(test.ZoneId, test.AppId);
            var trans = dbi.SqlDb.Database.BeginTransaction();

            // load an entity
            var loader1 = new Efc11Loader(dbi.SqlDb);
            var app1 = loader1.AppPackage(test.AppId);
            var itm1 = app1.Entities[test.ItemOnHomeId];

            // save it
            dbi.Entities.SaveEntity(itm1, so);

            // re-load it
            var loader2 = new Efc11Loader(dbi.SqlDb); // use existing db context because the transaction is still open
            var app2 = loader2.AppPackage(test.AppId);
            var itm2 = app2.Entities[test.ItemOnHomeId];


            // validate that they are still the same!
            Assert.AreEqual(itm1.Attributes.Count, itm2.Attributes.Count, "should have same amount of attributes");

            trans.Rollback();
        }

        [TestMethod]
        public void LoadOneChangeABitAndSave()
        {
            var test = new TestValuesOnPc2Dm();
            var so = SaveOptions.Build(test.ZoneId);
            so.PreserveUntouchedAttributes = true;
            so.PreserveUnknownLanguages = true;

            var dbi = DbDataController.Instance(test.ZoneId, test.AppId);
            var trans = dbi.SqlDb.Database.BeginTransaction();

            // todo: load a simple, 1 language entity
            var loader1 = new Efc11Loader(dbi.SqlDb);
            var app1 = loader1.AppPackage(test.AppId);
            var itm1 = app1.Entities[test.ItemOnHomeId];

            // todo: make some minor changes
            var itmNewTitle = new Entity(test.AppId, 0, "", new Dictionary<string, object>()
            {
                {"Title", "changed title"}
            });
            var saveEntity = EntitySaver.CreateMergedForSaving(itm1, itmNewTitle, so);

            // save it
            dbi.Entities.SaveEntity(saveEntity, so);

            // reload it
            var loader2 = new Efc11Loader(dbi.SqlDb); // use existing db context because the transaction is still open
            var app2 = loader2.AppPackage(test.AppId);
            var itm2 = app2.Entities[test.ItemOnHomeId];


            // todo: validate that they are almost the same, but clearly different
            Assert.AreEqual(itm1.Attributes.Count, itm2.Attributes.Count, "should have same amount of attributes");
            Assert.AreNotEqual(itm1.GetBestTitle(), itm2.GetBestTitle(), "title should have changed");

            trans.Rollback();
        }

        [TestMethod]
        public void CreateNewItemAndSave()
        {
            var ctName = "Simple Content";
            var ctTitle = "wonderful new title";
            var test = new TestValuesOnPc2Dm();
            var so = SaveOptions.Build(test.ZoneId);
            so.PreserveUntouchedAttributes = true;
            so.PreserveUnknownLanguages = true;

            var dbi = DbDataController.Instance(test.ZoneId, test.AppId);
            var trans = dbi.SqlDb.Database.BeginTransaction();

            // load content type to start creating an item...
            var loader1 = new Efc11Loader(dbi.SqlDb);
            var app1 = loader1.AppPackage(test.AppId);
            var ct1 = app1.ContentTypes.Values.First(ct => ct.Name == ctName);

            var newE = new Entity(test.AppId, 0, ct1.StaticName, new Dictionary<string, object>
            {
                { "Title", ctTitle }
            });
            newE.SetType(ct1);

            var saveEntity = EntitySaver.CreateMergedForSaving(null, newE, so);

            // save it
            var newId = dbi.Entities.SaveEntity(saveEntity, so);

            // reload it
            var loader2 = new Efc11Loader(dbi.SqlDb); // use existing db context because the transaction is still open
            var app2 = loader2.AppPackage(test.AppId);
            var itm2 = app2.Entities[newId];

            Assert.AreEqual(itm2.GetBestTitle(), ctTitle, "title should be loaded as saved" );

            trans.Rollback();


        }
    }
}
