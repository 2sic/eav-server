using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Data
{
    [TestClass]
    public class EntityTest: TestBaseEavCore
    {
        [TestMethod]
        public void Entity_CreateSimpleUnpersistedEntity()
        {
            var entDaniel = new SampleData(GetService<MultiBuilder>()).TestEntityDaniel();

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
            var entityBuilder = GetService<MultiBuilder>();
            var sampleData = new SampleData(entityBuilder);
            var dan = sampleData.TestEntityDaniel();
            var relDtoL = new EntityRelationship(dan, sampleData.TestEntityLeonie());
            var relationshipList = new List<EntityRelationship> {relDtoL};
            for (var p = 0; p < 15; p++)
            {
                var relPet = new EntityRelationship(dan, sampleData.TestEntityPet(p));
                relationshipList.Add(relPet);
            }

            // ReSharper disable once UnusedVariable
            var relMan = new RelationshipManager(dan, null, relationshipList);

            Assert.AreEqual(16, relMan.AllChildren.Count());
            // note: can't test more, because the other properties are internal
        }


    }
}
