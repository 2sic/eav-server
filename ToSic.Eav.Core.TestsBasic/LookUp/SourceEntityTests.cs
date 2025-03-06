using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.StartUp;
using ToSic.Lib;
using Xunit.DependencyInjection;
using static Xunit.Assert;

namespace ToSic.Eav.LookUp;

[Startup(typeof(TestStartupEavCore))]
public class SourceEntityTests(DataBuilder dataBuilder)
{
    [Fact]
    public void ValueProvider_EntityValueProvider()
    {
        var valProv = new LookUpInEntity("no-name", dataBuilder.TestEntityDaniel(), null);

        NotEqual(string.Empty, valProv.Get("FirstName")); //, "Has first name");
        NotEqual(string.Empty, valProv.Get(Attributes.EntityIdPascalCase));//, "Has entity id");
        NotEqual(string.Empty, valProv.Get(Attributes.EntityFieldTitle));//, "Has entity title");
        Equal("Mettler", valProv.Get("LastName", ""));
        Equal("Mettler", valProv.Get("LastName"));
        Equal(1.ToString(), valProv.Get(Attributes.EntityIdPascalCase));
        Equal("Daniel", valProv.Get(Attributes.EntityFieldTitle));
        // this test can't work, because ispublished is blank on a light entity
        // Assert.IsTrue(Convert.ToBoolean(valProv.Get("IsPublished")));
        Equal(Guid.Empty, Guid.Parse(valProv.Get("EntityGuid")));
        Equal("TestType", valProv.Get("EntityType"));
    }

    [Fact]
    public void Entity_EntityRelationship()
    {
        var dan = dataBuilder.TestEntityDaniel();

        var relationshipList = new List<EntityRelationship>
        {
            new(dan, dataBuilder.TestEntityLeonie())
        };

        for (var p = 0; p < 15; p++)
        {
            var relPet = new EntityRelationship(dan, dataBuilder.TestEntityPet(p));
            relationshipList.Add(relPet);
        }

        // ReSharper disable once UnusedVariable
        var relMan = new EntityRelationships(dan, null, relationshipList);

        Equal(16, relMan.AllChildren.Count());
        // note: can't test more, because the other properties are internal
    }

}