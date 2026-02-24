using ToSic.Eav.LookUp;
using static ToSic.Eav.DataSource.DbTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

[Startup(typeof(StartupTestFullWithDb))]
public class ParentsTests(DataSourcesTstBuilder dsSvc, LookUpTestData lookUpTestData)
    : ChildParentTestBase<Parents>(dsSvc, lookUpTestData), IClassFixture<DoFixtureStartup<ScenarioBasic>>
{

    [Fact]
    public void PersonsOneHasNoParents()
    {
        var cl = PrepareDs(RelationshipTestSpecs.Person, [PersonWithCompany], Company);
        Empty(cl.ListTac());
    }

    [Fact]
    public void CountrySwitzerlandHas2Parents()
    {
        var cl = PrepareDs(Country, [CountrySwitzerland]);
        Equal(CountrySwitzerlandParents, cl.ListTac().Count());
    }
        
}