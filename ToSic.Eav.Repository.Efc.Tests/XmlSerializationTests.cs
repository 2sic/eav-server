using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class XmlSerializationTests
    {
        private int AppId = 2;
        private int TestItemId = 0;

        [TestMethod]
        public void SerializeOneXml()
        {
            var test = new TestValuesOnPc2dm();
            var dbc = DbDataController.Instance(null, test.AppId);
            var xmlbuilder = new DbXmlBuilder(dbc);

            var xml = xmlbuilder.XmlEntity(test.ItemOnHomeId);
            
            var xmlstring = xml.ToString();
            Assert.IsTrue(xmlstring.Length > 200, "should get a long xml string");

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(test.AppId);
            var exBuilder = new XmlPersistence(app);
            var xmlEnt = exBuilder.XmlEntity(test.ItemOnHomeId).ToString();
            Assert.IsTrue(xmlEnt.Length > 200, "should get a long xml string");

            Assert.AreEqual(xmlstring, xmlEnt, "xml strings should be identical");
        }

        // [Ignore]
        [TestMethod]
        public void CompareAllSerializedEntitiesOfApp()
        {
            var test = new TestValuesOnPc2dm();
            var dbc = DbDataController.Instance(null, test.AppId);
            var xmlbuilder = new DbXmlBuilder(dbc);
            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(test.AppId);
            var exBuilder = new XmlPersistence(app);

            var maxCount = 1500;
            var skip = 0;
            var count = 0;
            foreach (var appEntity in app.Entities.Values)
            {
                // maybe skip some
                if (count++ < skip) continue;

                var xml = xmlbuilder.XmlEntity(appEntity.EntityId);
                var xmlstring = xml.ToString();
                var xmlEnt = exBuilder.XmlEntity(appEntity.EntityId).ToString();
                Assert.AreEqual(xmlstring, xmlEnt, $"xml of item {count} strings should be identical - but was not on xmlEnt {appEntity.EntityId}");

                // stop if we ran enough tests
                if (count >= maxCount)
                    return;
            }


        }
    }


}
