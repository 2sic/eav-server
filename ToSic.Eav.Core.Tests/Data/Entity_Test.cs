using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Data;

namespace ToSic.Eav.Core.Tests.Data;

[TestClass]
public class EntityTest: TestBaseEavCore
{
    [TestMethod]
    public void Entity_CreateSimpleUnpersistedEntity()
    {
        var entDaniel = new SampleData(GetService<DataBuilder>()).TestEntityDaniel();

        Assert.AreEqual(1, entDaniel.EntityId);
        Assert.AreEqual(Guid.Empty, entDaniel.EntityGuid);
        Assert.AreEqual("Daniel", entDaniel.Attributes["FirstName"].ValuesTac().FirstOrDefault()?.ObjectContents.ToString());
        Assert.AreEqual("Daniel", entDaniel.GetBestTitle());
        Assert.AreEqual("Daniel", entDaniel.TacGet("FirstName"));
        Assert.AreEqual("Daniel", entDaniel.TacGet<string>("FirstName"));
        Assert.AreEqual("Mettler", entDaniel.TacGet("LastName", languages: ["EN"]));
        Assert.AreEqual("Mettler", entDaniel.TacGet("LastName", language: "EN"));
        Assert.AreEqual("Mettler", entDaniel.TacGet<string>("LastName", languages: ["EN"]));
        Assert.AreEqual("Mettler", entDaniel.TacGet<string>("LastName", language: "EN"));
    }

    [TestMethod]
    public void Entity_EntityRelationship()
    {
        var entityBuilder = GetService<DataBuilder>();
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
        var relMan = new EntityRelationships(dan, null, relationshipList);

        Assert.AreEqual(16, relMan.AllChildren.Count());
        // note: can't test more, because the other properties are internal
    }


}