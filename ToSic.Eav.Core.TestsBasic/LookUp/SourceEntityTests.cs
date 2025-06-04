using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;
using ToSic.Eav.LookUp.Sources;
using Xunit.DependencyInjection;

namespace ToSic.Eav.LookUp;

[Startup(typeof(StartupTestsEavCore))]
public class SourceEntityTests(DataBuilder dataBuilder)
{
    private readonly LookUpInEntity _person = new("no-name", dataBuilder.TestEntityDaniel(), null);

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
    public void EntityTypeIsTestType() => Equal("TestType", _person.GetTac("EntityType"));

    [Fact]
    public void AnyDate() => Equal(DateTime.Parse(TestEntities.AnyDateString), DateTime.Parse(_person.GetTac(TestEntities.AnyDateKey)));

    /// <summary>
    /// TODO: This test has never been completed.
    /// It should start with an entity with sub-properties, and then allow the test to check the sub-property values.
    /// </summary>
    [Fact]
    public void SubPropertyTODO() // not quite done yet!
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

        // note: can't test more, because the other properties are internal
    }

}