using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.UnitTests.Data
{
    [TestClass]
    public class EntityTest
    {
        [TestMethod]
        public void Entity_CreateSimpleUnpersistedEntity()
        {
            var entDaniel = TestEntityDaniel();

            Assert.AreEqual(1, entDaniel.EntityId);
            Assert.AreEqual(Guid.Empty, entDaniel.EntityGuid);
            Assert.AreEqual("Daniel", entDaniel.Title[0].ToString());
            Assert.AreEqual("Daniel", entDaniel.GetBestTitle());
            Assert.AreEqual("Daniel", entDaniel.GetBestValue("FirstName"));
            Assert.AreEqual("Mettler", entDaniel.GetBestValue("LastName", new[] {"EN"}));
        }

        [TestMethod]
        public void Entity_EntityRelationship()
        {
            var dan = TestEntityDaniel();
            var relDtoL = new EntityRelationshipItem(dan, TestEntityLeonie());
            var relationshipList = new List<EntityRelationshipItem>();
            relationshipList.Add(relDtoL);
            for (var p = 0; p < 15; p++)
            {
                var relPet = new EntityRelationshipItem(dan, TestEntityPet(p));
                relationshipList.Add(relPet);
            }

            // ReSharper disable once UnusedVariable
            var relMan = new RelationshipManager(dan, relationshipList);

            // note: can't test more, because the other properties are internal
        }


        #region Test-Data (entities)
        public IEntity TestEntityDaniel()
        {
            var valDaniel = new Dictionary<string, object>()
            {
                {"FirstName", "Daniel"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 750 67 70"},
                {"Age", 37}
            };
            var entDaniel = new Eav.Data.Entity(1, "TestType", valDaniel, "FirstName");
            return entDaniel;
        }

        public IEntity TestEntityLeonie()
        {
            var valLeonie = new Dictionary<string, object>()
            {
                {"FirstName", "Leonie"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 xxx yy zz"},
                {"Age", 6}
            };

            var entLeonie = new Eav.Data.Entity(2, "TestType", valLeonie, "FirstName");
            return entLeonie;
        }
        public IEntity TestEntityPet(int petNumber)
        {
            var valsPet = new Dictionary<string, object>()
            {
                {"FirstName", "PetNo" + petNumber},
                {"LastName", "Of Bonsaikitten"},
                {"Phone", "+41 81 xxx yy zz"},
                {"Age", petNumber}
            };

            var entPet = new Eav.Data.Entity(1000+petNumber, "Pet", valsPet, "FirstName");
            return entPet;
        }
        #endregion
    }
}
