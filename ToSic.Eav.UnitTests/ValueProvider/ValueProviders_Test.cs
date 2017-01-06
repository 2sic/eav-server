using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.UnitTests.Data;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.UnitTests.ValueProvider
{
    [TestClass]
    public class ValueProvidersTest
    {
        [TestMethod]
        public void ValueProvider_StaticValueProvider()
        {
            var sv = new StaticValueProvider("Demo");
            sv.Properties.Add("Alpha", "found");
            sv.Properties.Add("Bravo", "found it too");
            sv.Properties.Add("Child:Grandchild", "found");

            bool found = false;

            Assert.IsTrue(sv.Has("Alpha"));
            Assert.IsTrue(sv.Has("alpha")); // true now that caps don't matter
            Assert.IsTrue(sv.Has("Bravo"));
            Assert.IsFalse(sv.Has("Charlie"));
            Assert.IsTrue(sv.Get("Alpha", "", ref found) == "found");
            Assert.IsTrue(sv.Get("Bravo", "", ref found) == "found it too");
            Assert.IsTrue(sv.Get("Child:Grandchild", "", ref found) == "found");
            Assert.IsTrue(sv.Get("Child", "", ref found) == null);
        }

        [TestMethod]
        public void ValueProvider_EntityValueProvider()
        {
            IValueProvider valProv = new EntityValueProvider(new EntityTest().TestEntityDaniel());
            bool found = false;

            Assert.IsTrue(valProv.Has("FirstName"), "Has first name");
            Assert.IsTrue(valProv.Has("EntityId"), "Has entity id");
            Assert.IsTrue(valProv.Has(Constants.EntityFieldTitle), "Has entity title");
            Assert.AreEqual("Mettler", valProv.Get("LastName", "", ref found));
            Assert.AreEqual("Mettler", valProv.Get("LastName"));
            Assert.AreEqual(1.ToString(), valProv.Get("EntityId"));
            Assert.AreEqual("Daniel", valProv.Get(Constants.EntityFieldTitle));
            Assert.IsTrue(Convert.ToBoolean(valProv.Get("IsPublished")));
            Assert.AreEqual(Guid.Empty, Guid.Parse(valProv.Get("EntityGuid")));
            Assert.AreEqual("TestType", valProv.Get("EntityType"));
        }

        [TestMethod]
        public void ValueProvider_EntityValueProvider_SubProperty_TODO()
        {
            // todo
        }

        //private IEntity GenerateSimpleTestEntity()
        //{
        //    var valDaniel = new Dictionary<string, object>()
        //    {
        //        {"FirstName", "Daniel"},
        //        {"LastName", "Mettler"},
        //        {"Phone", "+41 81 750 67 70"},
        //        {"Age", 37}
        //    };
        //    var valLeonie = new Dictionary<string, object>()
        //    {
        //        {"FirstName", "Leonie"},
        //        {"LastName", "Mettler"},
        //        {"Phone", "+41 81 xxx yy zz"},
        //        {"Age", 6}
        //    };

        //    var entDaniel = new Data.Entity(1, "TestType", valDaniel, "FirstName");
        //    //var entLeonie = new Data.Entity(2, "TestType", valLeonie, "FirstName");
        //    //var eri = new EntityRelationshipItem(entDaniel, entLeonie);
        //    //var Relationships = new ToSic.Eav.RelationshipManager(entDaniel, new EntityRelationshipItem[0]);
            


        //    return entDaniel;

        //}
    }
}
