using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Data;

[Startup(typeof(TestStartupEavCore))]
public class EntityRelationshipTests(DataBuilder dataBuilder)
{

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