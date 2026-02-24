using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.TestData;

namespace ToSic.Eav.Data.Relationships;

[Startup(typeof(StartupTestsEavDataBuild))]
public class EntityRelationshipTests(DataAssembler dataAssembler, ContentTypeAssembler typeAssembler)
{

    [Fact]
    public void Entity_EntityRelationship()
    {
        var dan = dataAssembler.TestEntityDaniel(typeAssembler);

        var relationshipList = new List<EntityRelationship>
        {
            new(dan, dataAssembler.TestEntityLeonie(typeAssembler))
        };

        for (var p = 0; p < 15; p++)
        {
            var relPet = new EntityRelationship(dan, dataAssembler.TestEntityPet(typeAssembler, p));
            relationshipList.Add(relPet);
        }

        // ReSharper disable once UnusedVariable
        var relMan = new EntityRelationships(dan, null, relationshipList);

        Equal(16, relMan.AllChildren.Count());
        // note: can't test more, because the other properties are internal
    }

}