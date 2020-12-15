using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.Core.Tests.Data
{
    [TestClass]
    public class EntityTest
    {
        [TestMethod]
        public void Entity_CreateSimpleUnpersistedEntity()
        {
            var entDaniel = SampleData.TestEntityDaniel();

            Assert.AreEqual(1, entDaniel.EntityId);
            Assert.AreEqual(Guid.Empty, entDaniel.EntityGuid);
            Assert.AreEqual("Daniel", entDaniel.Attributes["FirstName"].Values.FirstOrDefault()?.ObjectContents.ToString());// .Title[0].ToString());
            Assert.AreEqual("Daniel", entDaniel.GetBestTitle());
            Assert.AreEqual("Daniel", entDaniel.Value("FirstName"));
            Assert.AreEqual("Daniel", entDaniel.Value<string>("FirstName"));
            Assert.AreEqual("Mettler", entDaniel.GetBestValue("LastName", new[] {"EN"}));
        }

        [TestMethod]
        public void Entity_EntityRelationship()
        {
            var dan = SampleData.TestEntityDaniel();
            var relDtoL = new EntityRelationship(dan, SampleData.TestEntityLeonie());
            var relationshipList = new List<EntityRelationship> {relDtoL};
            for (var p = 0; p < 15; p++)
            {
                var relPet = new EntityRelationship(dan, SampleData.TestEntityPet(p));
                relationshipList.Add(relPet);
            }

            // ReSharper disable once UnusedVariable
            var relMan = new RelationshipManager(dan, null, relationshipList);

            // note: can't test more, because the other properties are internal
        }


    }
}
