using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.Data;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.LookUp;

[TestClass]
public class LookUpTests: TestBaseEavCore
{
    [TestMethod]
    public void ValueProvider_EntityValueProvider()
    {
        ILookUp valProv = new LookUpInEntity("no-name", GetService<DataBuilder>().TestEntityDaniel(), null);

        Assert.AreNotEqual(string.Empty, valProv.Get("FirstName"), "Has first name");
        Assert.AreNotEqual(string.Empty, valProv.Get(Attributes.EntityIdPascalCase), "Has entity id");
        Assert.AreNotEqual(string.Empty, valProv.Get(Attributes.EntityFieldTitle), "Has entity title");
        Assert.AreEqual("Mettler", valProv.Get("LastName", ""));
        Assert.AreEqual("Mettler", valProv.Get("LastName"));
        Assert.AreEqual(1.ToString(), valProv.Get(Attributes.EntityIdPascalCase));
        Assert.AreEqual("Daniel", valProv.Get(Attributes.EntityFieldTitle));
        // this test can't work, because ispublished is blank on a light entity
        // Assert.IsTrue(Convert.ToBoolean(valProv.Get("IsPublished")));
        Assert.AreEqual(Guid.Empty, Guid.Parse(valProv.Get("EntityGuid")));
        Assert.AreEqual("TestType", valProv.Get("EntityType"));
    }

    [TestMethod]
    public void ValueProvider_EntityValueProvider_DateTimeFormat()
    {
        ILookUp valProv = new LookUpInEntity("no-name", GetService<DataBuilder>().TestEntityDaniel(), null);
            
        Assert.AreEqual(DateTime.Parse("2019-11-06T01:00:05Z"), DateTime.Parse(valProv.Get("AnyDate")));
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