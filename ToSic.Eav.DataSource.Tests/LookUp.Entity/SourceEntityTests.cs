using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.TestData;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.TestHelpers;

namespace ToSic.Eav.LookUp.Entity;

[Startup(typeof(StartupTestsEavDataBuildWithTestData))]
public class SourceEntityTests(DataAssembler dataAssembler, ContentTypeAssemblyKit ctAssemblyKit)
{
    private readonly LookUpInEntity _person = new("no-name", dataAssembler.TestEntityDaniel(ctAssemblyKit), null);

    [Fact]
    public void FirstNameNotEmpty() => NotEqual(string.Empty, _person.GetTac("FirstName"));

    [Fact]
    public void EntityIdNotEmpty() => NotEqual(string.Empty, _person.GetTac(AttributeNames.EntityIdPascalCase));

    [Fact]
    public void EntityTitleNotEmpty() => NotEqual(string.Empty, _person.GetTac(AttributeNames.EntityFieldTitle));

    [Fact]
    public void LastNameIsMettler() => Equal("Mettler", _person.GetTac("LastName", ""));

    [Fact]
    public void LastNameIsMettlerWithoutDefault() => Equal("Mettler", _person.GetTac("LastName"));

    [Fact]
    public void EntityIdIs1() => Equal(1.ToString(), _person.GetTac(AttributeNames.EntityIdPascalCase));

    [Fact]
    public void EntityTitleIsDaniel() => Equal("Daniel", _person.GetTac(AttributeNames.EntityFieldTitle));

    [Fact]
    public void EntityGuidIsEmpty() => Equal(Guid.Empty, Guid.Parse(_person.GetTac(AttributeNames.EntityGuidPascalCase)));

    [Fact]
    public void EntityTypeIsTestType() => Equal("TestType", _person.GetTac(AttributeNames.EntityFieldType));

    [Fact]
    public void AnyDate() => Equal(DateTime.Parse(TestEntities.AnyDateString), DateTime.Parse(_person.GetTac(TestEntities.AnyDateKey)));

    /// <summary>
    /// TODO: This test has never been completed.
    /// It should start with an entity with sub-properties, and then allow the test to check the sub-property values.
    /// </summary>
    [Fact]
    public void SubPropertyTODO() // not quite done yet!
    {
        var dan = dataAssembler.TestEntityDaniel(ctAssemblyKit);

        var relationshipList = new List<EntityRelationship>
        {
            new(dan, dataAssembler.TestEntityLeonie(ctAssemblyKit))
        };

        for (var p = 0; p < 15; p++)
        {
            var relPet = new EntityRelationship(dan, dataAssembler.TestEntityPet(ctAssemblyKit, p));
            relationshipList.Add(relPet);
        }

        // ReSharper disable once UnusedVariable
        var relMan = new EntityRelationships(dan, null, relationshipList);

        // note: can't test more, because the other properties are internal
    }

}